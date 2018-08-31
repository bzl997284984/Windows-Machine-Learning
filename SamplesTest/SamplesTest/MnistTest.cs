﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

// Define an alias to OpenQA.Selenium.Appium.Interactions.PointerInputDevice to hide
// inherited OpenQA.Selenium.Interactions.PointerInputDevice that causes ambiguity.
// In the future, all functions of OpenQA.Selenium.Appium.Interactions should be moved
// up to OpenQA.Selenium.Interactions and this alias can simply be removed.
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace SamplesTest
{
    public class MnistSession
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string MNISTAppId_CS = "f330385a-7468-4688-859d-7d11a61d1b29_ms3keja78xfsy!App";

        protected static WindowsDriver<WindowsElement> session;

        public static void Setup(TestContext context)
        {
            if (session == null)
            {
                DesiredCapabilities appCapabilities = new DesiredCapabilities();
                appCapabilities.SetCapability("app", MNISTAppId_CS);
                appCapabilities.SetCapability("deviceName", "WindowsPC");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);
                Assert.IsNotNull(session);
                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times
                session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
            }
        }

        public static void TearDown()
        {
            if (session != null)
            {
                session.Quit();
                session = null;
            }
        }
    }

    [TestClass]
    public class MnistTest : MnistSession
    {
        private static WindowsElement inkCanvas;
        private static WindowsElement recognizeButton;
        private static WindowsElement clearButton;
        private static WindowsElement numberLabel;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Setup(context);
            inkCanvas = session.FindElementByClassName("InkCanvas");
            recognizeButton = session.FindElementByName("Recognize");
            clearButton = session.FindElementByXPath("//Button[@AutomationId=\"clearButton\"]");
            numberLabel = session.FindElementByAccessibilityId("numberLabel");
        }

        [TestMethod]
        public void TestDigits()
        {
            IList<uint> digits = new List<uint> { 0, 1, 4, 7, 8 };
            for (int i = 0; i < digits.Count; i++)
            {
                clearButton.Click();
                uint digit = digits[i];
                Draw(digit);
                recognizeButton.Click();
                Assert.AreEqual(numberLabel.Text, digit.ToString());
            }
        }

        public void DrawCircle(PointerInputDevice penDevice, ActionSequence sequence, int centerX, int centerY, int radius, int stepSize)
        {
            sequence.AddAction(penDevice.CreatePointerMove(
                CoordinateOrigin.Viewport,
                centerX + radius,
                centerY,
                TimeSpan.Zero
            ));
            sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
            for (int i = 0; i <= stepSize; i++)
            {
                sequence.AddAction(penDevice.CreatePointerMove(
                    CoordinateOrigin.Viewport,
                    Convert.ToInt32(centerX + radius * Math.Cos(2 * Math.PI * i / stepSize)),
                    Convert.ToInt32(centerY + radius * Math.Sin(2 * Math.PI * i / stepSize)),
                    TimeSpan.Zero
                ));
            }
            sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
        }

        // draws a given digit on canvas
        // (0,0)  (x,0)
        //  ┌───────┐   
        //  │       │   In viewport(default) origin mode:
        //  │       │   - X is absolute horizontal position in the session window
        //  └───────┘   - Y is absolute vertical position in the session window
        // (y,0)  (x,y) - (0,0) is on the top left corner, and (x,y) is on bottom right corner
        public void Draw(uint digit)
        {
            PointerInputDevice penDevice = new PointerInputDevice(PointerKind.Pen);
            ActionSequence sequence = new ActionSequence(penDevice, 0);
            Point canvasCoordiante = inkCanvas.Coordinates.LocationInViewport;
            switch (digit)
            {
                case 0:
                {
                    // drawing a circle around a center
                    // start from cos(0), sin(0)
                    var centerX = canvasCoordiante.X + inkCanvas.Size.Width / 2;
                    var centerY = canvasCoordiante.Y + inkCanvas.Size.Height / 2;
                    var radius = inkCanvas.Size.Width / 3;
                    var stepSize = 10;
                    DrawCircle(penDevice, sequence, centerX, centerY, radius, stepSize);
                    break;
                }
                case 1:
                    // (0,0)   (x,0)
                    //  ┌───────┐   Draw a basic line from top to bottom.
                    //  │   |   │  
                    //  │   |   │  
                    //  └───────┘ 
                    // (y,0)   (x,y)
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + inkCanvas.Size.Width / 2,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 9,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + inkCanvas.Size.Width / 2,
                        canvasCoordiante.Y + 8 * inkCanvas.Size.Height / 9,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    // (0,0)   (x,0)
                    //  ┌───────┐   
                    //  │  / |  │   
                    //  │ ───── |   
                    //  │    |  │   
                    //  └───────┘   
                    // (y,0)   (x,y)
                    // vertical line
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + 2 * inkCanvas.Size.Width / 3,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 8,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + 2 * inkCanvas.Size.Width / 3,
                        canvasCoordiante.Y + 5 * inkCanvas.Size.Height / 6,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
                    // horizontal line
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + inkCanvas.Size.Width / 4,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 2,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + 3 * inkCanvas.Size.Width / 4,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 2,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
                    // diagonal
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + inkCanvas.Size.Width / 4,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 2,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + 2 * inkCanvas.Size.Width / 3,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 8,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    // (0,0)   (x,0)
                    //  ┌───────┐   
                    //  │ ───── |   
                    //  │    /  │   
                    //  │   /   │   
                    //  └───────┘   
                    // (y,0)   (x,y)
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + inkCanvas.Size.Width / 6,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 6,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerDown(PointerButton.PenContact));
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + 6 * inkCanvas.Size.Width / 6,
                        canvasCoordiante.Y + inkCanvas.Size.Height / 6,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerMove(
                        CoordinateOrigin.Viewport,
                        canvasCoordiante.X + 3 * inkCanvas.Size.Width / 6,
                        canvasCoordiante.Y + 5 * inkCanvas.Size.Height / 6,
                        TimeSpan.Zero));
                    sequence.AddAction(penDevice.CreatePointerUp(PointerButton.PenContact));
                    break;
                case 8:
                {
                    // two circles. Smaller circle on top of bigger circle
                    var centerX = canvasCoordiante.X + inkCanvas.Size.Width / 2;
                    var centerY = canvasCoordiante.Y + inkCanvas.Size.Height / 5 + inkCanvas.Size.Height / 10;
                    var radius = inkCanvas.Size.Height / 6;

                    var stepSize = 10;
                    DrawCircle(penDevice, sequence, centerX, centerY, radius, stepSize);
                    centerX = canvasCoordiante.X + inkCanvas.Size.Width / 2;
                    centerY = canvasCoordiante.Y + 3 * inkCanvas.Size.Height / 5 + inkCanvas.Size.Height / 10;
                    radius = Convert.ToInt32(1.4 * inkCanvas.Size.Height / 6);
                    DrawCircle(penDevice, sequence, centerX, centerY, radius, stepSize);
                    break;
                }
                case 9:
                    break;
                default:
                    Assert.Fail("wrong argument");
                    break;
            }
            session.PerformActions(new List<ActionSequence> { sequence });
        }
    }
}