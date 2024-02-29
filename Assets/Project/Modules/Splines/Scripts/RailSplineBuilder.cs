using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

namespace EntilandVR.DosCuatro.CDI.G_Cuatro
{
    
    public class RailSplineBuilder : MonoBehaviour
    {
        [Header("SPLINE CONTAINERS")]
        [SerializeField] private SplineContainer _coreSplineContainer;
        [SerializeField] private SplineContainer _sideSplinesContainer;
        [SerializeField] private SplineInstantiate _coreSplineInstantiate;
        [SerializeField] private SplineExtrude _sidesSplineExtrude;

        [Header("CONFIGURATION")] 
        [SerializeField, Range(0, 10)] private int _extraResolution = 0;
        [SerializeField, Range(0f, 10.0f)] private float _railsWidth = 2.0f;
        private float _halfRailsWidth;
        [SerializeField] private GameObject _railPlankPrefab;

        private delegate void IterateCallback(float3 position, float3 tangent, float3 up, float3 normal);

        private Spline _coreSpline;
        private Spline _leftSpline;
        private Spline _rightSpline;

        
        private int NumberOfCorePoints => _coreSpline.Count;
        private int _numberOfPoints;


        private void OnValidate()
        {
            _halfRailsWidth = _railsWidth / 2;
            Start();
        }
        

        private void Start()
        {
            if (Application.isPlaying)
            {
                // TODO make this cleaner
                _sidesSplineExtrude.Container = _sideSplinesContainer;
                _sidesSplineExtrude.SegmentsPerUnit = 0.5f;
                _sidesSplineExtrude.Sides = 6;
                _sidesSplineExtrude.RebuildOnSplineChange = false;

                _coreSplineInstantiate.Container = _coreSplineContainer;
                _railPlankPrefab.transform.localScale = new Vector3(_railsWidth * 1.5f, 0.2f, 1.0f);
                SplineInstantiate.InstantiableItem plank = new SplineInstantiate.InstantiableItem
                {
                    Prefab = _railPlankPrefab,
                    Probability = 1
                };
                _coreSplineInstantiate.itemsToInstantiate = new[] { plank };
                _coreSplineInstantiate.MinSpacing = 6.0f;
            }

            
            
            Setup();
            BuildSideSplines();
            
            if (Application.isPlaying)
            {
                _sidesSplineExtrude.Rebuild();
            }
        }

        private void Update()
        {
            if (_leftSpline == null || _rightSpline == null)
            {
                Setup();
            }


            BuildSideSplines();
        }
        

        private void Setup()
        {
            _coreSpline = _coreSplineContainer.Splines[0];            
            
            _numberOfPoints = NumberOfCorePoints + ((NumberOfCorePoints - 1) * _extraResolution);
            
            _leftSpline = new Spline(_numberOfPoints);
            _rightSpline = new Spline(_numberOfPoints);
            
            _sideSplinesContainer.Splines = new[]
            {
                _leftSpline,
                _rightSpline
            };
        }


        private void BuildSideSplines()
        {
            _leftSpline.Clear();
            _rightSpline.Clear();
            
            IterateCoreSpline(AddLeftAndRightSplinePoints);

            _leftSpline.SetTangentMode(new SplineRange(0, _numberOfPoints), TangentMode.AutoSmooth);
            _rightSpline.SetTangentMode(new SplineRange(0, _numberOfPoints), TangentMode.AutoSmooth);
        }

        private void AddLeftAndRightSplinePoints(float3 position, float3 tangent, float3 up, float3 normal)
        {
            float3 leftPosition = position + (normal * _halfRailsWidth);
            BezierKnot leftKnot = new BezierKnot(leftPosition);
            _leftSpline.Add(leftKnot);
                
            float3 rightPosition = position - (normal * _halfRailsWidth);
            BezierKnot rightKnot = new BezierKnot(rightPosition);
            _rightSpline.Add(rightKnot);
        }


        private void OnDrawGizmos()
        {
            IterateCoreSpline(DoDrawGizmos);
        }
        
        private void DoDrawGizmos(float3 position, float3 tangent, float3 up, float3 normal)
        {            
            Gizmos.color = Color.cyan;
            Vector3 upPosition = position + up;
            Gizmos.DrawLine(position, upPosition);
                
                
            Gizmos.color = Color.green;
            Vector3 leftPosition = position + normal;
            Gizmos.DrawLine(position, leftPosition);
                
            Gizmos.color = Color.red;
            Vector3 rightPosition = position - normal;
            Gizmos.DrawLine(position, rightPosition);
            
            /*                
            Gizmos.color = Color.yellow;
            Vector3 tangentInPosition = position + tangent;
            Gizmos.DrawLine(position, tangentInPosition);
                
            Gizmos.color = Color.black;
            Vector3 tangentOutPosition = position - tangent;
            Gizmos.DrawLine(position, tangentOutPosition);

            */
        }


        
        
        private void IterateCoreSpline(IterateCallback iterateCallback)
        {
            int numberOfPointsMinusOne = _numberOfPoints - 1;

            float t = 0f;

            for (int i = 0; i < _numberOfPoints; ++i)
            {
                t = (float)i / numberOfPointsMinusOne;

                _coreSpline.Evaluate(t, out float3 position, out float3 tangent, out float3 up);
                
                
                tangent = new Vector3(tangent.x, tangent.y, tangent.z).normalized;
                
                float3 normal = Vector3.Cross(tangent, up).normalized;

                iterateCallback(position, tangent, up, normal);
            }
        }


        


        
    }
}


