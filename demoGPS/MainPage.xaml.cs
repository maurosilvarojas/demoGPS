using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace demoGPS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BusLocation newBusLocation = new BusLocation();
        private object textTime;
        private string timeNow;
        private MqttClient client;
        byte[] message;
        string Location;

        public MainPage()
        {
            this.InitializeComponent();
            DateTime localDate = DateTime.Now;
            textTime = localDate.ToString();
            timeNow = localDate.ToString();
            this.client = new MqttClient("broker.mqttdashboard.com");//QOS_LEVEL_AT_MOST_ONCE
            try
            {
                this.client.Connect(Guid.NewGuid().ToString());
                Debug.WriteLine("broker.mqttdashboard.com");
            }
            catch (Exception)
            {
                try
                {
                    this.client = new MqttClient("iot.eclipse.org");
                    this.client.Connect(Guid.NewGuid().ToString());
                    Debug.WriteLine("iot.eclipse.org");
                }
                catch (Exception)
                {

                    Debug.WriteLine("No Broker Connection");
                }
                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
                this.client.Publish("/MQTTpublisher", Encoding.UTF8.GetBytes("connected"));
            }
           //this.client.Subscribe(new string[] { "/busLocation" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            
            this.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void myMap_Loaded(object sender, RoutedEventArgs e)
        {
            if (myMap.Is3DSupported)
            {
                myMap.Style = MapStyle.Aerial3DWithRoads;
                //MyMap.Style = MapStyle.Terrain;
                myMap.MapServiceToken = "Your Service Token";

                BasicGeoposition geoPosition = new BasicGeoposition();
                geoPosition.Latitude = 52.250465;
                geoPosition.Longitude = -0.889620;
                // get position
                Geopoint myPoint = new Geopoint(geoPosition);
                //create POI
                MapIcon myPOI = new MapIcon { Location = myPoint, Title = "My Position", NormalizedAnchorPoint = new Point(0.5, 1.0), ZIndex = 0 };
                // Display an image of a MapIcon
                myPOI.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/pin.png"));
                // add to map and center it
                myMap.MapElements.Add(myPOI);
                myMap.Center = myPoint;
                myMap.ZoomLevel = 15;


                //MapScene mapScene = MapScene.CreateFromLocationAndRadius(new Geopoint(geoPosition), 500, 150, 70);
                //await myMap.TrySetSceneAsync(mapScene);
            }
        }

    

        private void myMap_Tapped(object sender, TappedRoutedEventArgs e)
        {
           
        }

        private void myMap_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            var tappedGeoPosition = args.Location.Position;
            newBusLocation.latitude = tappedGeoPosition.Latitude.ToString();
            newBusLocation.longitude = tappedGeoPosition.Longitude.ToString();
            newBusLocation.DateTime = timeNow.ToString();
            newBusLocation.busID = "19-001";
            myLocationText.Text = "long: " + tappedGeoPosition.Latitude + ",Lat:  " + tappedGeoPosition.Longitude;
            Debug.WriteLine(myLocationText.Text);
            publisherLocation();
        }

        private void publisherLocation()
        {           
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(newBusLocation);
            //  Newtonsoft.Json.JsonConvert.PopulateObject(json, newBusLocation);
            this.client.Publish("/busLocation", Encoding.UTF8.GetBytes(json));
            Debug.WriteLine("message published");
        }
    }
}


//JSON sample :

//{
//"BusLocation":
//            {
//              "busID": "19-001",
//              "Date-Time": "2014-06-25T00:00:00.000Z",
//              "driver-Name": true,
//              "latitude": "52.2500574868172",
//              "longitude": " -0.891518080607057"
//            }
//}

