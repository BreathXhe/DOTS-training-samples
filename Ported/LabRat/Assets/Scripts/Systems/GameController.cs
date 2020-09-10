﻿using Unity.Entities;
using Unity.Mathematics;

struct GameStateInitialize : IComponentData {}
struct GameStateStart : IComponentData {}
struct GameStateRunning : IComponentData {}
struct GameStateCleanup : IComponentData {}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class GameController : SystemBase
{
    const float GameDuration = 30;
    const float GameRestartDelay = 5;

    enum GameState { None, ApplicationStarting, GameInitializing, GameInitialized, GameStarting, GameStarted, GameRunning, GameEnding, GameRestarting, GameCleanup }

    EntityCommandBufferSystem m_EntityCommandBufferSystem;
    
    UIBridge m_UIBridge;
    
    GameState m_GameState;
    float m_TimeAccumulator;

    protected override void OnCreate()
    {
        // We'll sync our command buffers on the beginning of next frame's init group
        m_EntityCommandBufferSystem = World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        // Bind to hybrid UI
        m_UIBridge = UnityEngine.Object.FindObjectOfType<UIBridge>();

        // Set the starting state
        m_GameState = GameState.ApplicationStarting;
    }

    protected override void OnUpdate()
    {
        switch (m_GameState)
        {
            case GameState.ApplicationStarting:
            {
                m_GameState = GameState.GameInitializing;
                break;
            }

            case GameState.GameInitializing:
            {
                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.WithName("Leave_GameCleanup").WithAll<WantsGameStateTransitions>()
                    .ForEach((Entity entity) => ecb.RemoveComponent<GameStateCleanup>(entity)).Schedule();
                Entities.WithName("Enter_GameInit").WithAll<WantsGameStateTransitions>()
                    .ForEach((Entity entity) => ecb.AddComponent<GameStateInitialize>(entity)).Schedule();
                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

                m_GameState = GameState.GameInitialized;
                break;
            }
            
            case GameState.GameInitialized:
            {
                m_TimeAccumulator = 0f;
                
                m_UIBridge.ShowReady(() =>
                {
                    m_UIBridge.ShowSet(() =>
                    {
                        m_UIBridge.ShowGo();

                        m_GameState = GameState.GameStarting;
                    });
                });

                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.WithName("Leave_GameInit").WithAll<WantsGameStateTransitions, GameStateInitialize>()
                    .ForEach((Entity entity) => ecb.RemoveComponent<GameStateInitialize>(entity)).Schedule();
                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

                m_GameState = GameState.None;
                break;
            }

            case GameState.GameStarting:
            {
                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.WithName("Enter_GameStart").WithAll<WantsGameStateTransitions>()
                    .ForEach((Entity entity) => ecb.AddComponent<GameStateStart>(entity)).Schedule();
                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
                
                m_GameState = GameState.GameStarted;
                break;
            }

            case GameState.GameStarted:
            {
                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.WithName("Leave_GameStart").WithAll<WantsGameStateTransitions, GameStateStart>()
                    .ForEach((Entity entity) => ecb.RemoveComponent<GameStateStart>(entity)).Schedule();
                Entities.WithName("Enter_GameRunning").WithAll<WantsGameStateTransitions>()
                    .ForEach((Entity entity) => ecb.AddComponent<GameStateRunning> (entity)).Schedule();
                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
                
                m_GameState = GameState.GameStarted;
                goto case GameState.GameRunning;
            }

            case GameState.GameRunning:
            {
                m_TimeAccumulator += Time.DeltaTime;
                var gameTimeRemaining = math.max(0f, GameDuration - m_TimeAccumulator);
        
                m_UIBridge.SetTimer(gameTimeRemaining);
                
                var scores = new int[4];
                Entities.WithName("Score2UI").WithAll<Score>()
                    .ForEach((int entityInQueryIndex, in Score s) => scores[entityInQueryIndex] = s.Value).WithoutBurst().Run();
                for (int i = 0; i < 4; i++)
                    m_UIBridge.SetScore(i, scores[i]);

                if (gameTimeRemaining == 0f)
                    m_GameState = GameState.GameEnding;
                
                break;
            }

            case GameState.GameEnding:
            {
                var maxScore = -1;
                Entity winner = Entity.Null;
                Entities.WithName("FindHighScore").WithAll<Score>()
                    .ForEach((int entityInQueryIndex, Entity e, in Score s) =>
                    {
                        if (s.Value > maxScore)
                        {
                            maxScore = s.Value;
                            winner = e;
                        }
                    }).WithoutBurst().Run();
                var msg = "(no-clue)";
                UnityEngine.Color col = UnityEngine.Color.black;
                if (winner != Entity.Null)
                {
                    msg = EntityManager.GetComponentData<Name>(winner).Value.ToString();

                    var colComp = EntityManager.GetComponentData<Color>(winner).Value;
                    col = new UnityEngine.Color( colComp.x, colComp.y, colComp.z, 1 );
                }
                m_UIBridge.ShowGameOver(msg, col);
                m_TimeAccumulator = 0f;
                
                m_GameState = GameState.GameRestarting;
                break;
            }
            
            case GameState.GameRestarting:
            {
                m_TimeAccumulator += Time.DeltaTime;

                if (m_TimeAccumulator >= GameRestartDelay)
                {
                    m_UIBridge.ResetGUI();
                    m_GameState = GameState.GameCleanup;
                }
                
                break;
            }
            
            case GameState.GameCleanup:
            {
                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.WithName("Enter_GameCleanup").WithAll<WantsGameStateTransitions>()
                     .ForEach((Entity entity) => ecb.AddComponent<GameStateCleanup>(entity)).Schedule();
                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

                m_GameState = GameState.GameInitializing;
                break;
            }
        }
    }
}