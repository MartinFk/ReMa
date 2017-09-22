using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;

/* 
 author: Martin Feick
 project: baxter robot - rema
      -> via OptiTrack streaming-plugin
		-> also works with the arrow keys and a simple mouse (tests) 			 
 date: december 15, 2016
 last update: july 21, 2017
*/


public class Cube : MonoBehaviour
{
    /* 
		Connection variables
	*/
    private string message = "0\n0\n0\n0\n0\n0\n0"; //default-message
    private int PORT = 4242; //Open TCP port - Python server 
    private IPAddress baxterIP = IPAddress.Parse("10.0.0.7"); //Python server address
    private float nextSendingTime;
    private float sendingFrequency = 1.0F;
    private bool connecting = false; //connection status

    /* 
		Declaration of variables
	*/
    float rotSpeed = 120;
    float moveSpeed = 1;

    float epsilon = 0.02f; //avoiding hystereses - jittering

    //float resetTime = 0f;

    Quaternion pos;

    float actualX;
    float actualY;
    float actualZ;

    float actualXRot;
    float actualYRot;
    float actualZRot;

    float compareOrientationX;
    float compareOrientationY;
    float compareOrientationZ;
	
	float origin = 0.0f;

    /*
		Method for moving/spinning the dice 
	*/
    void OnMouseDrag()
    {
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

        transform.Rotate(Vector3.up, -rotX);
        transform.Rotate(Vector3.right, rotY);

    }

    void Start()
    {
        //default-position of the object
        Vector3 x = transform.position;
        actualX = x.x;
        actualY = x.y;
        actualZ = x.z;

        //default-orientation of the object
        actualXRot = 0;
        actualYRot = 0;
        actualZRot = 0;

        //default-quaternion 
        pos = getQuaternionFromEulerAngles(actualYRot, actualXRot, actualZRot);

        //initialize communication between client and server
        print("Starting...");
        this.nextSendingTime = Time.time;

    }


    void Update()
    {

        transform.Translate(moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime, 0f, moveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime); //Arrow keys 

        SendPackage(); //Every frame send a package (amount depends on your computer (FPS))

    }

	// 45 degrees mode
    public static float ChooseAngle(float temp)
    {
        if (temp > 337 && temp <= 22)
            return 0;
        if (temp > 22 && temp <= 67)
            return 45;
        if (temp > 67 && temp <= 112)
            return 90;
        if (temp > 112 && temp <= 157)
            return 135;
        if (temp > 157 && temp <= 202)
            return 180;
        if (temp > 202 && temp <= 247)
            return 225;
        if (temp > 247 && temp <= 292)
            return 270;
        if (temp > 292 && temp <= 337)
            return 315;

        return 0;
    }
	
	// 90 degrees mode
	  
        // if (temp > 315 && temp <= 45)
            // return 0;
        // if (temp > 45 && temp <= 135)
            // return 90;
        // if (temp > 135 && temp <= 225)
            // return 180;
        // if (temp > 225 && temp <= 315)
            // return 270;

        // return 0;
    

    void SendPackage() {
		
		 Vector3 xCur = transform.position; //getCurrent x,y,z - values
		
    if (Mathf.Abs(actualX - xCur.x) > epsilon || Mathf.Abs(actualY - xCur.y) > epsilon || Mathf.Abs(actualZ - xCur.z) > epsilon) {

       

        //getCurrent orientation angles
        float eulerCurX = transform.eulerAngles.x;
        float eulerCurY = transform.eulerAngles.y;
        float eulerCurZ = transform.eulerAngles.z;

        // compareOrientationX = ChooseAngle(eulerCurX);
        // compareOrientationY = ChooseAngle(eulerCurY);
        // compareOrientationZ = ChooseAngle(eulerCurZ);
		
		// shared-private workspace		
	   //if(xCur.x > origin) 
	
        // if (compareOrientationX != actualXRot)
        // {
            // actualXRot = compareOrientationX;
            // pos = getQuaternionFromEulerAngles(actualYRot, actualXRot, actualZRot);           
        // }

        // if (compareOrientationY != actualYRot)
        // {
            // actualYRot = compareOrientationY;
            // pos = getQuaternionFromEulerAngles(actualYRot, actualXRot, actualZRot);
        // }

        // if (compareOrientationZ != actualZRot)
        // {
            // actualZRot = compareOrientationZ;
            // pos = getQuaternionFromEulerAngles(actualYRot, actualXRot, actualZRot);
        // }


        //  --------------------------------------TEST-----------------------------------------------

        //Testing - significant coordinates change 

      
            actualX = xCur.x;
            actualY = xCur.y;
            actualZ = xCur.z;
  
    }
	
	Send();
	
	}

    void Send()
    {

        //build message - has to be in this sequence -> baxter's standart frame
        this.message = "2\n" + (actualX) + "\n" + (actualY*-1f)  + "\n" + actualZ + "\n" + 0 + "\n" + 1 + "\n" + 0 + "\n" + 0 + "\n"; //"\n" + pos.w + "\n" + pos.x + "\n" + pos.y + "\n" + pos.z + "\n"

		//Debugging
        print(actualX);
        print(actualY);
        print(actualZ);
        print(pos.w);
        print(pos.x);
        print(pos.y);
        print(pos.z);

        //Sending a TCP-message to the Python server (position and orientation update)
        if (this.nextSendingTime < Time.time)
        {
            this.ConnectAndSend();
            this.nextSendingTime = Time.time + 1.0F / this.sendingFrequency;
        }
    }

    public static Quaternion getQuaternionFromEulerAngles(float yaw, float pitch, float roll)
    {
        yaw *= Mathf.Deg2Rad;
        pitch *= Mathf.Deg2Rad;
        roll *= Mathf.Deg2Rad;

        float rollOver2 = roll * 0.5f;
        float sinRollOver2 = (float)Math.Sin((double)rollOver2);
        float cosRollOver2 = (float)Math.Cos((double)rollOver2);

        float pitchOver2 = pitch * 0.5f;
        float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
        float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);

        float yawOver2 = yaw * 0.5f;
        float sinYawOver2 = (float)Math.Sin((double)yawOver2);
        float cosYawOver2 = (float)Math.Cos((double)yawOver2);

        Quaternion quaternion;

        quaternion.w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
        quaternion.x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
        quaternion.y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
        quaternion.z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

        return quaternion;
    }

    // --> uncomment for using rotation matrix

    //void QuatToMatrix(Quaternion q)  
    //{
    //    double sqw = q.w * q.w;
    //    double sqx = q.x * q.x;
    //    double sqy = q.y * q.y;
    //    double sqz = q.z * q.z;

    //    double m00, m11, m22, m10, m01, m20, m02, m21, m12;

    //    // invs (inverse square length) is only required if quaternion is not already normalised
    //    // double invs = 1 / (sqx + sqy + sqz + sqw);
    //    m00 = (sqx - sqy - sqz + sqw);
    //    m11 = (-sqx + sqy - sqz + sqw);
    //    m22 = (-sqx - sqy + sqz + sqw);

    //    double tmp1 = q.x * q.y;
    //    double tmp2 = q.z * q.w;
    //    m10 = 2.0 * (tmp1 + tmp2);
    //    m01 = 2.0 * (tmp1 - tmp2);

    //    tmp1 = q.x * q.z;
    //    tmp2 = q.y * q.w;
    //    m20 = 2.0 * (tmp1 - tmp2);
    //    m02 = 2.0 * (tmp1 + tmp2);
    //    tmp1 = q.y * q.z;
    //    tmp2 = q.x * q.w;
    //    m21 = 2.0 * (tmp1 + tmp2);
    //    m12 = 2.0 * (tmp1 - tmp2);

    //    print(m00);
    //    print(m10);
    //    print(m20);
    //    print("Column2");
    //    print(m01);
    //    print(m11);
    //    print(m21);
    //    print("Column3");
    //    print(m02);
    //    print(m12);
    //    print(m22);

    //}


    /* 
	The following two methods are adapted from the C-Sharp documentation and from Pascal Ziegler (UdS)
	*/
    void ConnectAndSend()
    {
        if (!this.connecting)
        {

            this.connecting = true;
            print("Connection starts...");

            try
            {
                TcpClient client = new TcpClient();
                client.BeginConnect(this.baxterIP, this.PORT, new System.AsyncCallback(this.SendAfterConnect), client);
                print("connected");
            }
            catch (System.ArgumentException e)
            {
                print("ArgumentNullException: {0}" + e);
                this.connecting = false;
            }
            catch (SocketException e)
            {
                print("SocketException: {0}" + e);
                this.connecting = false;
            }

        }

    }

    private void SendAfterConnect(System.IAsyncResult asyncResult)
    {

        print("connection successful");

        TcpClient client = (TcpClient)asyncResult.AsyncState;

        byte[] data = System.Text.Encoding.ASCII.GetBytes(this.message);

        NetworkStream stream = client.GetStream();

        print("stream opened");

        stream.Write(data, 0, data.Length);

        print("Sent to baxter: " + this.message);

        stream.Close();
        print("stream closed");
        client.Close();
        print("client closed");
        this.connecting = false;
    }

}