using System.Collections.Generic;
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

        public void Deserialize(BitBuffer buffer)
        {
            packetNumber = buffer.GetInt();
            List<CubeEntity> news = new List<CubeEntity>();
            entities.ForEach(c =>
            {
                CubeEntity aux = new CubeEntity(c.GameObject, c.Id);
                aux.Deserialize(buffer);
                news.Add(aux);
            });
            entities = news;
        }

        public static Snapshot createInterpolationSnapshot(Snapshot previous, Snapshot next, float time, int id)
        {
            List<CubeEntity> cubeEntities = new List<CubeEntity>();
            for (int i = 0; i < previous.entities.Count; i++)
            {
                if (previous.entities[i].Id != id)
                {
                    cubeEntities.Add(CubeEntity.createInterpolationEntity(previous.entities[i], next.entities[i], time));
                }
            }
            return new Snapshot(-1, cubeEntities);
        }

        public int GetPacketNumber()
        {
            return packetNumber;
        }

        public void applyChanges()
        {    
            this.entities.ForEach(c => c.applyChanges());;
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