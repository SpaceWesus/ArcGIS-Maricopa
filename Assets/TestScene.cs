using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Geometry;
using Esri.Unity;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

// [ExecuteAlways]

public class TestScene : MonoBehaviour
{
    // API key used to get access to the Data from the SDK
    public string APIKey = "AAPK124a6069bbe1497c806a907a6ebc42eeqwAS8w-XQD9yDIOJ4GfulEXoFXSDjroT9poO_XqDuzPOmmQhA8pu3zrwz3zqmYqL";


    /* In this part of the tutorial, you will use ArcGIS Map component to create an ArcGISMap with the local 
     * mode. ArcGIS Map component has the necessary elements to create an ArcGIS Map and communicates with 
     * High Precision Framework to set the Origin Position where its value is specified with the ArcGISPoint. 
     * All geographically located objects need to be a parent of the game object that has the ArcGIS Map 
     * component attached.
       In this tutorial, use the following values for the Origin Position for New York City.*/

    // New York City values
    private ArcGISMapComponent arcGISMapComponent;
    private ArcGISPoint geographicCoordinates = new ArcGISPoint(-74.054921, 40.691242, 3000, ArcGISSpatialReference.WGS84());

    // Numbers from RIT Video
    // Rochester Ontario Lake
    // Long, Lat, Alt
    // -77.735482, 43.331219, 10

    /*ArcGIS Camera will load better LODs for areas closer to the view and lower LODs for the areas that are 
     * further away. Set up a Camera component variable under the main class: */
    private ArcGISCameraComponent cameraComponent;


    /*The method below will find if there is an existing ArcGIS Map component in the scene when the script is 
     * attached to a game object. Otherwise, it will create a game object named ArcGISMap with an ArcGIS Map 
     * component attached. You can set the ArcGISMapType for the ArcGISMap with the ArcGIS Map component. 
     * We are creating a Local scene for this tutorial example.*/
    private void CreateArcGISMapComponent()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();

        if (!arcGISMapComponent)
        {
            var arcGISMapGameObject = new GameObject("ArcGISMap");
            arcGISMapComponent = arcGISMapGameObject.AddComponent<ArcGISMapComponent>();
        }

        arcGISMapComponent.OriginPosition = geographicCoordinates;
        arcGISMapComponent.MapType = Esri.GameEngine.Map.ArcGISMapType.Local;

        arcGISMapComponent.MapTypeChanged += new ArcGISMapComponent.MapTypeChangedEventHandler(CreateArcGISMap);
    }


    /* ArcGIS Map is a container of GIS content and is used with ArcGIS Renderer to visualize the content 
     * that is included in the ArcGIS Map component. Set up a method (in this case CreateArcGISMap()) 
     * that creates the ArcGISMap container */
    public void CreateArcGISMap()
    {
        var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(arcGISMapComponent.MapType);

        /* To set up a basemap, use the ArcGISBasemap class. There are three ways to create a basemap to use 
         * in your ArcGIS Map. In this tutorial, use ArcGISImagery of ArcGISBasemapStyle class to use a 
         * basemap from the basemap styles service with the API key that is configured in the previous 
         * section. You can also set the API key directly in the constructor instead of using a global 
         * variable. For other ways to create a basemap, refer to the Basemap section.*/
        arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap(Esri.GameEngine.Map.ArcGISBasemapStyle.ArcGISImagery, APIKey);

        /*Use ArcGISMapElevation to create relief with elevation sources and their names. Use 
         * ArcGISImageElevationSource to use an elevation layer as the source. In this tutorial, use the 
         * Terrain 3D elevation layer from Esri.*/
        arcGISMap.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Terrain 3D", ""));

        // addition of the image layers for the map. Think of it like when you zoom into google maps and you see the tiling. These are essentially NYCs textures from above.
        var layer_1 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/UrbanObservatory_NYC_TransitFrequency/MapServer", "MyLayer_1", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_1);

        var layer_2 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/New_York_Industrial/MapServer", "MyLayer_2", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_2);

        var layer_3 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/4yjifSiIG17X0gW4/arcgis/rest/services/NewYorkCity_PopDensity/MapServer", "MyLayer_3", 1.0f, true, "");
        arcGISMap.Layers.Add(layer_3);

        // Same thing as the image layers, but these are the 3d buildings that are loaded in when you zoom in enough.
        var buildingLayer = new Esri.GameEngine.Layers.ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, "");
        arcGISMap.Layers.Add(buildingLayer);

        /* Use ArcGISExtentCircle in the CreateArcGISMap() method to set up a circular extent. The code below 
         * will create a circle extent based on the parameter values and attach it to the map's clipping area.*/
        arcGISMapComponent.EnableExtent = true;

        var extentCenter = new Esri.GameEngine.Geometry.ArcGISPoint(-74.054921, 40.691242, 3000, ArcGISSpatialReference.WGS84());
        var extent = new ArcGISExtentCircle(extentCenter, 10000);

        arcGISMap.ClippingArea = extent;
        /* If you want to configure a rectangular extent, use ArcGISExtentRectangle instead of 
         * ArcGISExtentCircle and configure (extentCenter, "WIDTH", "HEIGHT").*/

        //assigns the ArcGIS map object to the View.
        arcGISMapComponent.View.Map = arcGISMap;

    }


    /* The method below will first check whether there is an existing camera game object with an ArcGIS Camera
     * and a Location component respectively. If not, those components will be added to a Camera game object
     * that has a MainCamera tag to enable map rendering, player movement, and tile loading.
     * A rebase component is also attached in case of exploring large areas.*/
    private void CreateArcGISCamera()
    {
        cameraComponent = Camera.main.gameObject.GetComponent<ArcGISCameraComponent>();

        if (!cameraComponent)
        {
            var cameraGameObject = Camera.main.gameObject;

            cameraGameObject.transform.SetParent(arcGISMapComponent.transform, false);

            cameraComponent = cameraGameObject.AddComponent<ArcGISCameraComponent>();

            cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();

            cameraGameObject.AddComponent<ArcGISRebaseComponent>();
        }

        var cameraLocationComponent = cameraComponent.GetComponent<ArcGISLocationComponent>();

        if (!cameraLocationComponent)
        {
            cameraLocationComponent = cameraComponent.gameObject.AddComponent<ArcGISLocationComponent>();

            cameraLocationComponent.Position = geographicCoordinates;
            cameraLocationComponent.Rotation = new ArcGISRotation(65, 68, 0);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        CreateArcGISMapComponent();
        CreateArcGISCamera();
        // CreateSkyComponent();
        CreateArcGISMap();
    }

} 
