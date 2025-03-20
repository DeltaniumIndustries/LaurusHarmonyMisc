using XRL.UI;

public static class LaurusHarmonyConfigHandler
{
    public static bool LOGGING_HARMONY_BPSC => GetBooleanOption("OptionLaurusHarmonyLoggingBPSC");
    public static bool LOGGING_HARMONY_RENDER => GetBooleanOption("OptionLaurusHarmonyLoggingRender");

    private static bool GetBooleanOption(string optionKey) =>
        Options.GetOption(optionKey).EqualsNoCase("Yes");
}
