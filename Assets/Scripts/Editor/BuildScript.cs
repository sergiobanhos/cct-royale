using UnityEditor;

public static class BuildScript{
    [MenuItem("Build/Build Client and Server")]
    public static void BuildClientAndServer()
    {
        var scenes = new[] {
            "Assets/Scenes/ServerBootstrap.unity",
            "Assets/Scenes/LoginScene.unity",
            "Assets/Scenes/MenuScene.unity",
            "Assets/Scenes/BattleScene.unity",
            "Assets/Scenes/MatchmakingScene.unity"
        };

        // Build client
        BuildPipeline.BuildPlayer(scenes, "Builds/macOSClient/CCTRoyale.app",
            BuildTarget.StandaloneOSX, BuildOptions.None);

        // Build server (headless)
        BuildPipeline.BuildPlayer(scenes, "Builds/macOSServer",
            BuildTarget.StandaloneOSX, BuildOptions.EnableHeadlessMode);
    }

    [MenuItem("Build/BuildServer")]
    public static void BuildServer()
    {
        var scenes = new[] {
            "Assets/Scenes/ServerBootstrap.unity",
            "Assets/Scenes/LoginScene.unity",
            "Assets/Scenes/MenuScene.unity",
            "Assets/Scenes/BattleScene.unity",
            "Assets/Scenes/MatchmakingScene.unity"
        };
        // Build server (headless)
        BuildPipeline.BuildPlayer(scenes, "Builds/macOSServer",
            BuildTarget.StandaloneOSX, BuildOptions.EnableHeadlessMode);
    }
}
