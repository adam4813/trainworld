using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class TrainEngine : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private TrainEngineScriptableObject trainEngineScriptableObject;
    [SerializeField] private TrainStation dockedStation;
    [SerializeField] private TrainCargo cargo;
    [SerializeField] private SplineAnimate splineAnimate;
    public bool IsMoving => splineAnimate.IsPlaying;
    public TrainCargo Cargo => cargo;
    public TrainEngineScriptableObject TrainEngineScriptableObject => trainEngineScriptableObject;

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    private void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.MaxSpeed = Speed;
    }

    public void SetSplinePath(TrainTrack.TrackSplinePath splinePath)
    {
        splineAnimate.Container = splinePath.SplineContainer;
        splineAnimate.enabled = splineAnimate.Container.Spline.Knots.Any();
        
        if (splinePath.IsReversed)
        {
            splineAnimate.Container.ReverseFlow(0);
        }
        
        if (!dockedStation && splineAnimate.enabled)
        {
            splineAnimate.Restart(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out TrainStation trainStation)) return;

        if (!dockedStation)
        {
            StartCoroutine(DockAtStation(trainStation));
        }
    }

    public Vector3 EngineOffset()
    {
        return transform.forward * .5f;
    }

    public TrainCargo UnloadCargo()
    {
        var tempCargo = cargo;
        cargo = null;
        return tempCargo;
    }

    public void LoadCargo(TrainCargo stationCargo)
    {
        cargo = stationCargo;
    }

    private IEnumerator DockAtStation(TrainStation trainStation)
    {
        splineAnimate.Pause();

        while (trainStation.DockedTrainEngine && trainStation.DockedTrainEngine != this)
        {
            yield return new WaitForSeconds(1f);
        }

        trainStation.OnTrainDocked(this);
        dockedStation = trainStation;
    }

    public void FinishedDocking(TrainStation trainStation)
    {
        trainStation.OnTrainDeparted(this);
        dockedStation = null;

        if (splineAnimate.Container?.Spline?.Knots.Any() != null)
        {
            splineAnimate.Play();
        }
    }

    public void ClearSpline()
    {
        splineAnimate.Container = null;
    }
}