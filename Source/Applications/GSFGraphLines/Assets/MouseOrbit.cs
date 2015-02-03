//******************************************************************************************************
//  MouseOrbit.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/29/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using UnityEngine;

// ReSharper disable once CheckNamespace
public class MouseOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 50.0F;
    public float xSpeed = 300.0F;
    public float ySpeed = 300.0F;
    public int zoomRate = 50;
    public int mouseDownFrames = 10;
    public float minX = -9999.0F;
    public float arrowSpeed = 0.15F;
    public bool isActive = true;

    private float x;
    private float y;
    private float xOffset;
    private float yOffset;
    private bool buttonDown;
    private bool rotate;
    private int downCount;

    public MouseOrbit()
    {
        yOffset = 0.0F;
        xOffset = 0.0F;
    }

    // Use this for initialization
    protected void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (target && isActive)
        {
            // Handle pinch gesture
            if (Input.touchCount >= 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 curDist = touch0.position - touch1.position;
                Vector2 prevDist = (touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition);

                float delta = (curDist.magnitude - prevDist.magnitude) / (zoomRate / 5.0F);
                distance -= delta;
            }
            else
            {
                // Mouse button functions also work for touch input on Android
                if (Input.GetMouseButtonDown(0))
                {
                    buttonDown = true;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    buttonDown = false;
                }

                // Only start rotation after a few frames - this allows human
                // mutitouch interaction a moment to enage since it's rare that
                // two fingers will actually hit the screen at the exact same time
                if (buttonDown)
                {
                    downCount++;
                    rotate = (downCount >= mouseDownFrames);
                }
                else
                {
                    downCount = 0;
                    rotate = false;
                }

                if (rotate)
                {
                    if (Input.mousePosition.x > minX)
                    {
                        x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                        y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
                    }
                }

                distance += -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(distance);
            }

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 distance3 = new Vector3(0.0F, 0.0F, -distance);
            Vector3 position = rotation * distance3 + target.position;

            // Handle X/Y camera offset movement based on arrow keys
            if (Input.GetKey(KeyCode.RightArrow))
                xOffset -= arrowSpeed;
            else if (Input.GetKey(KeyCode.LeftArrow))
                xOffset += arrowSpeed;

            if (Input.GetKey(KeyCode.UpArrow))
                yOffset -= arrowSpeed;
            else if (Input.GetKey(KeyCode.DownArrow))
                yOffset += arrowSpeed;

            position.x += xOffset;
            position.y += yOffset;

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}
