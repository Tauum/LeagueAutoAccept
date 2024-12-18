public class GameState
{
    // StateFlags field for managing game state
    public StateFlags Flags;

    // Constructor to initialize Flags
    public GameState(StateFlags flags)
    {
        Flags = flags;
    }

    // Struct to hold game state flags
    public struct StateFlags
    {
        public bool IsAutoAcceptOn { get; set; }
        public bool PickedChamp { get; set; }
        public bool LockedChamp { get; set; }
        public bool PickedBan { get; set; }
        public bool LockedBan { get; set; }
        public bool PickedSpell1 { get; set; }
        public bool PickedSpell2 { get; set; }
        public bool SentChatMessages { get; set; }

        // Constructor to initialize all flags
        public StateFlags(bool initialValue)
        {
            IsAutoAcceptOn = initialValue;
            PickedChamp = initialValue;
            LockedChamp = initialValue;
            PickedBan = initialValue;
            LockedBan = initialValue;
            PickedSpell1 = initialValue;
            PickedSpell2 = initialValue;
            SentChatMessages = initialValue;
        }
    }

    // Reset state method
    public void ResetState()
    {
        // Reset Flags to a new StateFlags instance with all fields set to false
        Flags = new StateFlags(false);
    }
}