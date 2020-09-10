﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class AttractingSystem : SystemBase
{
    private EntityQuery m_TeamABees;
    private EntityQuery m_TeamBBees;
    
    private Random m_Random;
    private EntityCommandBufferSystem m_ECBSystem;
    
    protected override void OnCreate()
    {
        
        m_TeamABees = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<TeamA>(),
                ComponentType.ReadOnly<Bee>(),
            },
            None = new []
            {
                ComponentType.ReadOnly<Dying>(), 
                ComponentType.ReadOnly<Agony>(), 
            }
        });
        
        m_TeamBBees = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<TeamB>(),
                ComponentType.ReadOnly<Bee>(),
            }, None = new []
            {
                ComponentType.ReadOnly<Dying>(), 
                ComponentType.ReadOnly<Agony>(), 
            }
        });
        
        m_Random = new Random( 7 );
        m_ECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        /*
        var random = new Random( (uint)m_Random.NextInt() );
        
        var ecb = m_ECBSystem.CreateCommandBuffer();


            int teamABeesEntitiesLength = m_TeamABees.CalculateEntityCount();
            int teamBBeesEntitiesLength = m_TeamBBees.CalculateEntityCount();

            //if(teamABeesEntitiesLength > 0){}

            var beeEntities_TeamA =
                m_TeamABees.ToEntityArrayAsync(Allocator.TempJob, out var beeAEntitiesHandle);
            var beeEntities_TeamB =
                m_TeamBBees.ToEntityArrayAsync(Allocator.TempJob, out var beeBEntitiesHandle);

            Dependency = JobHandle.CombineDependencies(Dependency, beeAEntitiesHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, beeBEntitiesHandle);

            // go attacking here
            Entities.WithAll<TeamA>()
                .WithDisposeOnCompletion( beeEntities_TeamB )
                .WithAll<Idle>()
                .ForEach( ( Entity bee ) =>
            {
                
                int targetIndex = random.NextInt( 0, teamBBeesEntitiesLength );

                if (targetIndex < teamBBeesEntitiesLength)
                {
                    ecb.RemoveComponent<Idle>( bee );
                    ecb.AddComponent<Attack>( bee );
                    ecb.AddComponent( bee, new TargetEntity { Value = beeEntities_TeamB[targetIndex] } );
                }
            } ).Schedule();
            
            Entities.WithAll<TeamB>()
                .WithDisposeOnCompletion( beeEntities_TeamA )
                .WithAll<Idle>()
                .ForEach( ( Entity bee ) =>
                {   

                    int targetIndex = random.NextInt( 0, teamABeesEntitiesLength );

                    if (targetIndex < teamABeesEntitiesLength)
                    {
                        ecb.RemoveComponent<Idle>( bee );
                        ecb.AddComponent<Attack>( bee );
                        ecb.AddComponent( bee, new TargetEntity { Value = beeEntities_TeamA[targetIndex] } );
                    }
                       

                } ).Schedule();
        
        m_ECBSystem.AddJobHandleForProducer(Dependency);
        */
    }
}