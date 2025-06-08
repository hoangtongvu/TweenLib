using TweenLib.ShakeTween.Data;
using Unity.Burst;

namespace TweenLib.ShakeTween.Logic
{
    [BurstCompile]
    public static class ShakeDataHelper
    {
        [BurstCompile]
        public static int AddToList(
            ref ShakeDataList dataList
            , in ShakeDataIdPool idPool
            , in ShakeData newShakeData)
        {
            bool hasReusableId = idPool.Value.TryDequeue(out int id);

            if (hasReusableId)
            {
                dataList.Value[id] = newShakeData;
            }
            else
            {
                id = dataList.Value.Length;
                dataList.Value.Add(newShakeData);
            }

            return id;
        }

        [BurstCompile]
        public static void RemoveFromList(
            in ShakeDataList dataList
            , in ShakeDataIdPool idPool
            , in int id)
        {
            ref var shakeData = ref dataList.Value.ElementAt(id);
            shakeData.IsEnabled = 0;

            idPool.Value.Enqueue(id);
        }

    }

}
