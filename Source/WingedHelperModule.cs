namespace Celeste.Mod.WingedHelper;

public class WingedHelperModule : EverestModule {
    public static WingedHelperModule Instance { get; private set; }

    public WingedHelperModule() 
    {
        Instance = this;
        #if DEBUG
            Logger.SetLogLevel(nameof(WingedHelperModule), LogLevel.Verbose);
        #else
            Logger.SetLogLevel(nameof(TestModule), LogLevel.Info);
        #endif
    }

    public override void Load()
    {
        On.Celeste.Bumper.UpdatePosition += WingComponent.onBumperWiggle;
    }

    public override void Unload()
    {
        On.Celeste.Bumper.UpdatePosition -= WingComponent.onBumperWiggle;
    }
}
