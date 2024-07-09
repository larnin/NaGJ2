using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BuildingOperationCenter : BuildingBase
{
    public BuildingOperationCenter(BuildingInfos infos, GameLevel level) : base(infos, level)
    {

    }

    public override void Process(float deltaTime)
    {
        var resources = m_level.beltSystem.ContainerGetAllResources(m_ID, 0);

        foreach(var r in resources)
        {
            var nb = m_level.beltSystem.ContainerGetResourceNb(r, m_ID, 0);

            m_level.beltSystem.ContainerRemoveResource(r, nb, m_ID, 0);

            if (ResourceDataEx.IsLiquid(r))
                continue;

            m_level.resources.Add(r, nb);
        }
    }
}
