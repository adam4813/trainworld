using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainStation : GridBuildable
{
    [SerializeField] private List<CargoScriptableObject> availableCargo;
    [SerializeField] private Vector2Int stationSize;
    [SerializeField] private TrainTrack connectedTrack;
    [SerializeField] private TrainCargo inCargo;
    [SerializeField] private TrainCargo outCargo;
    [SerializeField] private TrainEngine dockedTrainEngine;

    public TrainEngine DockedTrainEngine => dockedTrainEngine;

    private void Awake()
    {
        UpdateRect(stationSize);
        StartCoroutine(GetNewCargo());
    }

    public void OnTrainDocked(TrainEngine trainEngine)
    {
        dockedTrainEngine = trainEngine;
        StartCoroutine(HandleTrainCargo(trainEngine));
    }

    private IEnumerator HandleTrainCargo(TrainEngine trainEngine)
    {
        // Todo: Wait until cargo is sold, if already has cargo
        if (trainEngine.Cargo?.Destination == this)
        {
            yield return new WaitForSeconds(1f);
            inCargo = trainEngine.UnloadCargo();
        }

        if (outCargo != null)
        {
            yield return new WaitForSeconds(1f);
            trainEngine.LoadCargo(outCargo);
        }

        trainEngine.FinishedDocking(this);
    }

    public void OnTrainDeparted(TrainEngine trainEngine)
    {
        dockedTrainEngine = null;
        // Do something when the train leaves the station

        if (inCargo != null)
        {
            SellCargo(inCargo);
            inCargo = null;
        }

        StartCoroutine(GetNewCargo());
    }

    private void SellCargo(TrainCargo trainCargo)
    {
        if (!trainCargo.CargoScriptableObject) return;

        PopularityManager.PopularityChanged(trainCargo.CargoScriptableObject.CargoValue);
    }

    private IEnumerator GetNewCargo()
    {
        yield return new WaitForSeconds(5f);
        var nextCargo = availableCargo[Random.Range(0, availableCargo.Count)];
        var destination = TrackManager.Instance.GetNextStation(this, connectedTrack);
        outCargo = new TrainCargo { CargoScriptableObject = nextCargo, Destination = destination };
    }

    public override Vector2Int GetSize()
    {
        return new Vector2Int(stationSize.x, stationSize.y);
    }
}