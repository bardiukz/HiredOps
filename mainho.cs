using System;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

using UnityEngine;
using Assets.Scripts.Engine.Network;
using Assets.Scripts.Game;

namespace HiredOps
{
    public class MainHO : MonoBehaviour
    {
        public int smooth = 2;
        public int fov = 70;
        private bool _teleport;

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _teleport = !_teleport;
            }

            //just testing this long time ago, when server list was "blocked" by the developers to all users.
            Assets.Scripts.Engine.CVars.MatchmakingMaxPing = 1000;
            Assets.Scripts.Engine.CVars.IsAlphaEnabled = true;
            Assets.Scripts.Engine.CVars.IsServersActive = true;
            Debug.developerConsoleVisible = true;

            var game = UnityNetworkConnection.ClientGame;
            var l_player = game.LocalPlayer;
            float m_dist = 99999;

            Vector2 AimTarget = Vector2.zero;

            try
            {
                foreach (EntityNetPlayer player in game.AlivePlayers)
                {
                    Transform[] a_child = player.transform.GetComponentsInChildren<Transform>();
                    foreach (Transform child in a_child)
                    {
                        if (!player.IsTeammate(l_player))
                        {
                            if (child.name == "NPC_Head")
                            {
                                var pos = Camera.main.WorldToScreenPoint(child.transform.position);

                                if (pos.z > -8)
                                {
                                    float dist = System.Math.Abs(Vector2.Distance(new Vector2(pos.x, Screen.height - pos.y), new Vector2((Screen.width / 2), (Screen.height / 2))));

                                    if (dist < fov)
                                    {
                                        if (dist < m_dist)
                                        {
                                            m_dist = dist;
                                            AimTarget = new Vector2(pos.x, Screen.height - pos.y);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (AimTarget != Vector2.zero)
                {
                    double d_x = AimTarget.x - Screen.width / 2.0f;
                    double d_y = AimTarget.y - Screen.height / 2.0f;

                    //aimsmooth
                    d_x /= smooth;
                    d_y /= smooth;

                    if (Input.GetKey(KeyCode.Mouse1))
                    {
                        mouse_event(0x0001, (int)d_x, (int)d_y, 0, 0);
                    }
                }
            }
            catch { }

            // ESP / RECOIL

            if (game)
            {
                foreach (EntityNetPlayer player in game.AlivePlayers)
                {
                    if (!player.IsTeammate(l_player))
                    {
                        //weeb esp/highlight in top of every enemy.
                        player.playerInfo.highlighted = true;
                        if (_teleport)
                        {
                            if (player == l_player || player.Immortal)
                                continue;

                            l_player.PlayerTransform.position = player.PlayerTransform.position;
                        }
                    }
                }

                //No recoil/hand shake.
                l_player.Ammo.ShouldShakeCamera = false;
                l_player.Ammo.CurrentWeapon.recoilSettings.AimingFactor = 0f;
                l_player.Ammo.CurrentWeapon.recoilSettings.NoAimingFactor = 0f;
                l_player.Ammo.CurrentWeapon.recoilSettings.AimingOpticFactor = 0f;
                l_player.Ammo.CurrentWeapon.recoilSettings.enabledFovChange = false;
                l_player.Ammo.CurrentWeapon.recoilSettings.moveCameraUpCurve = AnimationCurve.EaseInOut(0f, 0f, 0f, 0f);

                //testing purposes
                Assets.Scripts.Engine.CVars.ShowMinimap = true;
                l_player.ActualThermalVision.enabled = true;

            }
        }
    }
}
