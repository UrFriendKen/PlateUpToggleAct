using Controllers;
using HarmonyLib;
using Kitchen;
using System.Reflection;

namespace KitchenToggleAct.Patches
{
    [HarmonyPatch]
    internal static class PerformCommandRouter_Patch
    {
        static MethodBase TargetMethod()
        {
            return typeof(PerformCommandRouter).GetMethod(nameof(PerformCommandRouter.BroadcastCommand)).MakeGenericMethod(typeof(UserInputUpdate));
        }

        static void Prefix(ref UserInputUpdate update)
        {
            if (update.SourceIdentifier.Value == InputSourceIdentifier.Identifier)
            {
                if (update.Data.State.InteractAction == ButtonState.Pressed)
                {
                    PatchController.OnActPressed(update.Data.User);
                }
                else if (PatchController.ShouldActHold(update.Data.User))
                {
                    update.Data.State.InteractAction = ButtonState.Held;
                }

                if (update.Data.State.GrabAction == ButtonState.Pressed || update.Data.State.SecondaryAction2 == ButtonState.Pressed)
                {
                    PatchController.OnGrabPressed(update.Data.User);
                }
            }
        }
    }
}
