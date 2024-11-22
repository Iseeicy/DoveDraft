using Godot;

namespace DoveDraft.Input;

/// <summary>
/// TODO - Look. I know this class is nasty. I'm going to refactor this ASAP now that it's in C#, but I wanna make sure the port works first.
/// </summary>
[Tool, GlobalClass]
public partial class DoveDraftInputs : RefCounted
{
    public class Player
    {
        public class Move
        {
            public const string Forward = "player_mode_forward";
            public const string Backward = "player_mode_backward";
            public const string Left = "player_mode_left";
            public const string Right = "player_mode_right";

            public static InputAxis2D Axis { get; private set; } =
                new InputAxis2D(new InputAxis1D(Left, Right), new InputAxis1D(Forward, Backward));

            public static void RegisterInputs()
            {
                new ProjectInputBinder(Forward).BindKeyCode(Key.W);
                new ProjectInputBinder(Backward).BindKeyCode(Key.S);
                new ProjectInputBinder(Left).BindKeyCode(Key.A);
                new ProjectInputBinder(Right).BindKeyCode(Key.D);
            }

            public static void UnregisterInputs()
            {
                ProjectInputBinder.Remove(Forward);
                ProjectInputBinder.Remove(Backward);
                ProjectInputBinder.Remove(Left);
                ProjectInputBinder.Remove(Right);
            }
        }

        public class Look
        {
            public const string Up = "player_look_up";
            public const string Down = "player_look_down";
            public const string Left = "player_look_left";
            public const string Right = "player_look_right";

            public static InputAxis2D Axis { get; private set; } =
                new InputAxis2D(new InputAxis1D(Left, Right), new InputAxis1D(Down, Up));

            public static void RegisterInputs()
            {
            }

            public static void UnregisterInputs()
            {
            }
        }

        public class Item
        {
            public class Drop
            {
                public const string Stack = "player_drop_item_stack";
                public const string Single = "player_drop_item_single";

                public static void RegisterInputs()
                {
                    new ProjectInputBinder(Stack).BindKeyCode(Key.W,
                        new ProjectInputBinder.BindKeyCodeOptions() { ShiftPressed = true });
                    new ProjectInputBinder(Single).BindKeyCode(Key.W);
                }

                public static void UnregisterInputs()
                {
                    ProjectInputBinder.Remove(Stack);
                    ProjectInputBinder.Remove(Single);
                }
            }

            public class Scroll
            {
                public const string Forward = "player_scroll_item_forward";
                public const string Backward = "player_scroll_item_backward";

                public static void RegisterInputs()
                {
                    new ProjectInputBinder(Forward).BindMouseButton(MouseButton.WheelDown);
                    new ProjectInputBinder(Backward).BindMouseButton(MouseButton.WheelUp);
                }

                public static void UnregisterInputs()
                {
                    ProjectInputBinder.Remove(Forward);
                    ProjectInputBinder.Remove(Backward);
                }
            }

            public class Use
            {
                public const string Primary = "player_use_item_0";
                public const string Secondary = "player_use_item_1";

                public static void RegisterInputs()
                {
                    new ProjectInputBinder(Primary).BindMouseButton(MouseButton.Left);
                    new ProjectInputBinder(Secondary).BindMouseButton(MouseButton.Right);
                }

                public static void UnregisterInputs()
                {
                    ProjectInputBinder.Remove(Primary);
                    ProjectInputBinder.Remove(Secondary);
                }
            }

            public static void RegisterInputs()
            {
                Drop.RegisterInputs();
                Scroll.RegisterInputs();
                Use.RegisterInputs();
            }

            public static void UnregisterInputs()
            {
                Drop.UnregisterInputs();
                Scroll.UnregisterInputs();
                Use.UnregisterInputs();
            }
        }

        public const string Sprint = "player_should_run";
        public const string Jump = "player_jump";
        public const string Crouch = "player_crouch";
        public const string Interact = "player_interact";

        public static void RegisterInputs()
        {
            Move.RegisterInputs();
            Look.RegisterInputs();
            Item.RegisterInputs();
            new ProjectInputBinder(Sprint).BindKeyCode(Key.Shift);
            new ProjectInputBinder(Jump).BindKeyCode(Key.Space);
            new ProjectInputBinder(Crouch).BindKeyCode(Key.Ctrl);
            new ProjectInputBinder(Interact).BindKeyCode(Key.E);
        }

        public static void UnregisterInputs()
        {
            Move.UnregisterInputs();
            Look.UnregisterInputs();
            Item.UnregisterInputs();
            ProjectInputBinder.Remove(Sprint);
            ProjectInputBinder.Remove(Jump);
            ProjectInputBinder.Remove(Crouch);
            ProjectInputBinder.Remove(Interact);
        }
    }

    //
    //  Public Static Methods
    //

    public static void AddToInputMap()
    {
        Player.RegisterInputs();
        ProjectSettings.Save();
    }

    public static void RemoveFromInputMap()
    {
        Player.UnregisterInputs();
        ProjectSettings.Save();
    }
}