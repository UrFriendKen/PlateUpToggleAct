using Controllers;
using Kitchen;
using KitchenToggleAct.Patches;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenToggleAct
{
    public class ToggleActIndicatorView : MonoBehaviour
    {
        private int PlayerID = 0;

        public PlayerView PlayerView;

        public GameObject Indicator;

        FieldInfo f_ViewData = typeof(PlayerView).GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);

        public void Update()
        {
            if (PlayerID == 0)
            {
                if (PlayerView != null)
                {
                    object obj = f_ViewData?.GetValue(PlayerView);
                    if (obj != null)
                    {
                        PlayerView.ViewData viewData = (PlayerView.ViewData)obj;
                        if (viewData.InputSource == InputSourceIdentifier.Identifier)
                        {
                            PlayerID = viewData.PlayerID;
                        }
                    }
                }
                return;
            }
            Indicator.SetActive((NetworkRouter_Patch.ActHolds?.Contains(PlayerID) ?? false) || (PatchController.PlayerActHolds?.ContainsKey(PlayerID) ?? false));
        }
    }
}
