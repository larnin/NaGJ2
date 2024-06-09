using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class GameWorld
{
    readonly object m_worldsLock = new object();
    List<GameOneWorld> m_worlds = new List<GameOneWorld>();
    GameOneWorld m_currentWorld;

    readonly object m_timerLock = new object();
    float m_timer = 0;
    float m_threadUpdate = 0.1f;

    Thread m_thread;
    bool m_stopped = false;

    public void Init(float threadUpdate)
    {
        m_threadUpdate = threadUpdate;
        CreateAllWorlds();
        StartThread();
    }

    public void Update(float deltaTime)
    {
        lock (m_timerLock)
        {
            m_timer += deltaTime;
        }

        if (m_currentWorld != null)
            m_currentWorld.Update(deltaTime);
    }

    public void Destroy()
    {
        m_stopped = true;
        if (m_thread != null)
            m_thread.Join();
    }

    void CreateAllWorlds()
    {
        var levels = Global.instance.allLevels;
        foreach(var l in levels.levels)
        {
            GameOneWorld oneWorld = new GameOneWorld(l);
            oneWorld.Init();
            m_worlds.Add(oneWorld);
        }
    }

    void StartThread()
    {
        m_thread = new Thread(new ThreadStart(ThreadLoop));
        m_thread.Start();
    }

    void ThreadLoop()
    {
        while (!m_stopped)
        {
            float time = 0;
            lock (m_timerLock)
            {
                time = m_timer;
                m_timer = 0;
            }

            lock (m_worldsLock)
            {
                foreach (var world in m_worlds)
                    world.Update(time);
            }

            lock (m_timerLock)
            {
                time = m_timer;
            }

            float waitTime = m_threadUpdate - time;
            if(waitTime > 0.001f) //1ms
                Thread.Sleep((int)(waitTime * 1000));
        }
    }
}
