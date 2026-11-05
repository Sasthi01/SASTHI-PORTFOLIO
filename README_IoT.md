# IoT-Based Soil Moisture Monitoring and Irrigation Decision Support System

## Project Overview

This project is an Internet of Things (IoT)-based soil moisture
monitoring system developed to support garden irrigation decisions. A
Grove resistive soil moisture sensor is connected to a Raspberry Pi
through an external analogue-to-digital converter (ADC). The Raspberry
Pi acquires and processes sensor readings, then sends monitoring data
over a local Wi-Fi network to a backend application for storage and
presentation.

The system provides **monitoring and decision support**. It classifies
soil moisture conditions and indicates when irrigation may be required.
It does **not** automatically activate a water pump or irrigation valve.

## Project Objectives

-   Acquire soil moisture readings using a physical sensor.
-   Convert the sensor's analogue output into digital readings using an
    external ADC.
-   Process raw readings into a relative soil moisture percentage.
-   Classify moisture conditions to support irrigation decisions.
-   Transmit readings from the Raspberry Pi to a backend service.
-   Store measurements in a relational database (MYSQL).
-   Present current and historical monitoring information through a web
    dashboard.

## System Architecture

The project uses a distributed architecture:

1.  **Sensing layer:** A Grove resistive soil moisture sensor detects
    changes in soil moisture.
2.  **Conversion layer:** A DFRobot Gravity 16-bit ADC DFR0553
    (ADS1115-compatible) converts the sensor's analogue signal into a
    digital value.
3.  **Edge layer:** A Raspberry Pi 5 acquires ADC readings over I²C and
    calculates the relative moisture percentage and condition.
4.  **Communication layer:** The Raspberry Pi sends readings over Wi-Fi
    to a Flask REST API using an HTTP POST request.
5.  **Backend and storage layer:** The Flask application, hosted on a
    Windows laptop, receives readings and stores them in a MySQL
    database.
6.  **Presentation layer:** A web dashboard displays the latest
    measurements, historical readings and irrigation guidance.

## Hardware Components

  -----------------------------------------------------------------------
  Component                           Purpose
  ----------------------------------- -----------------------------------
  Raspberry Pi 5 (8 GB)               Edge device for sensor acquisition
                                      and processing

  Grove resistive soil moisture       Detects changes in soil moisture
  sensor v1.4                         

  DFRobot Gravity 16-bit ADC DFR0553  Converts the sensor's analogue
                                      output into digital readings

  Jumper wires                        Connect the sensor, ADC and
                                      Raspberry Pi

  64 GB USB flash drive               Storage device used for the
                                      Raspberry Pi operating system
  -----------------------------------------------------------------------

https://github.com/Sasthi01/SASTHI-PORTFOLIO/blob/IoT-Based-Soil-Moisture-Monitoring-and-Irrigation-Decision-Support-System/images/setUp.jpg

The ADC communicates with the Raspberry Pi using I²C. The sensor signal
is connected to ADC input A0.

## Software and Technologies

-   **Python** -- sensor acquisition, moisture calculations and data
    transmission.
-   **`smbus2`** -- I²C communication between the Raspberry Pi and ADC.
-   **Flask** -- backend application and REST API.
-   **MySQL** -- relational database for application data and sensor
    measurements.
-   **Flask-Login** -- user session management for the web application.
-   **HTML, CSS and JavaScript** -- dashboard interface.
-   **Chart.js** -- visualisation of monitoring data.

## Sensor Processing and Moisture Classification

The system converts the raw ADC value into a relative soil moisture
percentage using two calibration reference values:

-   Dry reference: `1210`
-   Wet reference: `13990`

The calculation is:

``` text
Moisture (%) = ((Raw Reading - 1210) / (13990 - 1210)) * 100
```

The calculated value is limited to the range 0--100%.

  Relative moisture   Classification   Irrigation guidance
  ------------------- ---------------- --------------------------
  Below 30%           DRY              Irrigation required
  30% to below 70%    MODERATE         Continue monitoring
  70% or higher       WET              Irrigation not indicated

**Important:** This percentage is a relative moisture index based on the
project's calibration references. It is not a laboratory measurement of
volumetric water content (VWC). The system provides irrigation guidance
rather than automatic irrigation control. 

## Data Flow

1.  The Raspberry Pi reads the sensor signal through the ADC using I²C.
2.  The software converts the raw ADC reading into a relative moisture
    percentage.
3.  The moisture condition is classified as DRY, MODERATE or WET.
4.  The reading and associated monitoring information are sent to the
    Flask REST API over the local network.
5.  The backend stores received measurements in MySQL.
6.  The dashboard presents the latest readings and historical trends.

## Database

The MySQL database used by the backend is named `comp717_iot`. Its
tables include:

-   `Irrigation_Zone`
-   `Sensor`
-   `Sensor_Data`
-   `Users`
  
## Testing and Demonstration

The project involved testing the sensor-to-ADC connection, acquiring
readings on the Raspberry Pi, processing moisture values, and
transmitting readings to the Flask backend. End-to-end monitoring
sessions were used to check that readings reached the backend and could
be viewed through the application.

## Limitations

-   The moisture percentage is a relative index, not a direct VWC
    measurement.
-   Resistive probes can be affected by electrode corrosion, soil
    conditions and measurement drift.
-   Data transmission depends on local network connectivity.
-   The Flask backend and MySQL database depend on the availability of
    the Windows laptop hosting them.
-   The current system provides recommendations only; it does not
    control a physical irrigation actuator.
-   The system has not been established as a large-scale or
    geographically distributed deployment.

## Possible Future Improvements

-   Improve calibration using controlled soil samples and a repeatable
    reference procedure.
-   Investigate sensor drift and reduce probe corrosion through improved
    measurement and power-management arrangements.
-   Add a physical relay-controlled pump or valve with appropriate
    electrical and safety protections.
-   Deploy the backend to a continuously available server or cloud
    platform.
-   Add longer-term monitoring, alerts and additional environmental
    sensors.
-   Evaluate irrigation recommendations against measured water use and
    plant or soil outcomes.

## Academic Project

Developed as an IoT project for the COMP717A module. The project
demonstrates sensor interfacing, edge processing, network communication,
backend integration, relational data storage and web-based monitoring.
