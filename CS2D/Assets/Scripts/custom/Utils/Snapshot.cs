using System.Collections.Generic;
using custom.Client;
using custom.Network;
using UnityEngine;

namespace custom.Utils
{
    public class Snapshot
    {
        private List<CubeEntity> entities;
        private int packetNumber;

        public Snapshot(int packetNumber, List<CubeEntity> entities)
        {
            this.entities = entities;
            this.packetNumber = packetNumber;
        }

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(packetNumber);
            entities.ForEach(c => c.Serialize(buffer));
        }

        public void Deserialize(BitBuffer buffer, ClientMessenger cm)
        {
            packetNumber = buffer.GetInt();
            List<CubeEntity> news = new List<CubeEntity>();
            while (buffer.HasRemaining())
            {
                CubeEntity aux = new CubeEntity(null, -1);
                aux.DeserializeSpecific(buffer, entities, cm);
                news.Add(aux);
            }
            entities = news;
        }

        public static Snapshot createInterpolationSnapshot(Snapshot previous, Snapshot next, float time, int id, ClientMessenger cm)
        {
            List<CubeEntity> cubeEntities = new List<CubeEntity>();
            for (int i = 0; i < previous.entities.Count; i++)
            {
                int nextId = next.entities[i].Id;
                if (nextId != id)
                {
                    if (cm.isIdRegistered(nextId))
                    {
                        cubeEntities.Add(CubeEntity.createInterpolationEntity(previous.entities[i], next.entities[i], time));
                    }
                    else
                    {
                        // cubeEntities.Add(CubeEntity.createInterpolationEntity(new CubeEntity(go, nextId), next.entities[i], time));
                    }
                }
            }
            return new Snapshot(-1, cubeEntities);
        }

        public int GetPacketNumber()
        {
            return packetNumber;
        }

        public void applyChanges(int id)
        {    
            this.entities.ForEach(c =>
            {
                if (c.Id != id)
                {
                    c.applyChanges();
                }
            });;
        }

        public CubeEntity getEntityById(int id)
        {
            foreach (var aux in entities)
            {
                if (aux.Id == id)
                {
                    return aux;
                }
            }

            return null;
        }
    }
}