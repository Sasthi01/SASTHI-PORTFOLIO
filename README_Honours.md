# Stage-Based Nutrition Recommendation System for Lactating Mothers 

**Honours Research Project | 2026**  
**University of KwaZulu-Natal**

An interpretable machine-learning and mathematical optimisation system designed to generate personalised nutritional targets and daily meal recommendations for breastfeeding mothers across different stages of lactation.

---

## Project Overview

Nutritional requirements can vary throughout the breastfeeding period, yet general dietary recommendations do not always account for differences in maternal characteristics and lactation stages.

This project developed a web-based nutrition recommendation system that integrates **Generalised Additive Models (GAMs)** with **Mixed-Integer Linear Programming (MILP)** to estimate personalised energy and protein requirements and translate them into practical daily meal recommendations.

The system combines maternal information, reference-based micronutrient requirements, food composition data and individual dietary preferences to generate structured meal plans.

The main objective was to create an integrated recommendation framework that is both interpretable and capable of accounting for nutritional requirements, dietary restrictions and practical food choices.

## Key Features

- **Stage-aware nutritional modelling:** Estimates daily energy and protein requirements based on maternal characteristics and lactation stage.
- **Interpretable machine learning:** Uses separate GAM models for energy and protein estimation.
- **Micronutrient planning:** Incorporates reference-based vitamin and mineral requirements for lactating women.
- **MILP meal optimisation:** Selects suitable foods and portion quantities while satisfying applicable nutritional constraints.
- **Dietary personalisation:** Supports unrestricted, vegetarian and vegan dietary configurations.
- **Allergy and food exclusions:** Filters unsuitable foods according to user-specified allergies and excluded ingredients.
- **Cuisine preferences:** Incorporates cuisine preferences into food recommendations.
- **Curated recipes:** Includes 211 curated recipes across eight cuisine categories.
- **Nutritional validation:** Evaluates generated meal plans against applicable nutritional targets and dietary restrictions.
- **Web-based interface:** Allows users to enter maternal information and receive personalised daily meal recommendations.

## Datasets Used

### 1. Demographic and Health Survey (DHS)

The machine-learning component used Demographic and Health Survey data from five African countries:

- Burundi (2016–2017)
- Ethiopia (2024–2025)
- Kenya (2022)
- Nigeria (2024)
- South Africa (2016)

After data preprocessing and participant selection, the final dataset contained **12,176 breastfeeding women**.

Key model inputs included:

- Maternal age
- Maternal weight
- Maternal height
- Parity
- Pregnancy status
- Child age
- Lactation stage
- Country

Lactation was classified into three stages:

| Stage | Breastfeeding duration |
|---|---|
| Early | 0–6 months |
| Mid | 7–12 months |
| Late | More than 12 months |

### 2. USDA FoodData Central

Food composition data were obtained from the **USDA FoodData Central** database, specifically:

- FoodData Central Foundation Foods
- Food and Nutrient Database for Dietary Studies (FNDDS)

The selected and processed food catalogue contains approximately **4,806 food records**.

The data provided nutritional information, including energy, protein, vitamins and minerals, which was used to calculate the nutritional contributions of selected foods.

### 3. Curated Recipe Dataset

A separate curated recipe dataset was developed containing **211 recipes across eight cuisine categories**.

Recipes were linked to USDA ingredient records to calculate their nutritional contributions and incorporate them into the recommendation process.

This expanded the available food combinations and improved the variety of generated meal plans.

## Methodology

### 1. Generalised Additive Models (GAMs)

Two separate Generalised Additive Models were developed:

- **Energy GAM:** Estimates daily energy requirements.
- **Protein GAM:** Estimates daily protein requirements.

GAMs were selected because they can model non-linear relationships while retaining interpretability.

The training targets were generated using established nutritional equations because directly observed individual energy and protein requirements were not available in the DHS dataset.

The protein target was calculated using a weight-based formula of 1.1 g/kg/day.

Energy targets were generated using an Estimated Energy Requirement (EER) framework incorporating maternal characteristics and lactation-stage adjustments.

### 2. Micronutrient Requirements

Reference-based nutritional requirements for lactating women were incorporated independently of the GAM predictions.

These included relevant vitamins and minerals such as:

- Vitamin A
- B vitamins
- Vitamin C
- Calcium
- Iron
- Magnesium
- Selenium
- Zinc

The applicable nutrient targets were passed to the optimisation component to support more comprehensive nutritional planning.

### 3. Mixed-Integer Linear Programming (MILP)

A MILP optimisation model was implemented using **PuLP with the CBC solver**.

The model determines:

- Which foods and recipes should be selected.
- How much of each selected food should be included.
- How the selected foods should be distributed across daily meals.

The optimisation incorporates:

- Energy and protein constraints.
- Applicable hard micronutrient requirements.
- Portion-size bounds.
- Breakfast, lunch, snack and supper structure.
- Optional dessert selection.
- Supporting-food and variety constraints.
- Food-selection preferences and practical penalties.

Energy is constrained to remain within 98–102% of the GAM-derived target, while protein is constrained to remain within 95–105%.

Dietary types, allergies and excluded foods are applied through food-candidate filtering before optimisation.

Vitamin D is treated as a soft planning target rather than a mandatory feasibility constraint, allowing the system to identify potential nutritional shortfalls.

### 4. Web Application Development

The recommendation pipeline was integrated into a Flask-based web application.

Users can enter their maternal characteristics and specify dietary preferences, allergies, cuisine preferences and excluded foods.

The backend processes these inputs, generates nutritional targets, filters the food catalogue, executes the MILP optimisation and presents the resulting daily meal recommendations.

## My Contributions

I was responsible for the development of the food recommendation and optimisation components, as well as the integration of the system into the web application.

### Food Data Processing and Curation

- Processed and curated USDA FoodData Central datasets.
- Cleaned and structured food composition records for optimisation.
- Developed and maintained the food catalogue used by the recommendation engine.
- Developed the curated recipe dataset containing 211 recipes across eight cuisine categories.
- Linked recipes to USDA ingredients for nutritional calculations.

### Micronutrient Module

- Incorporated reference-based micronutrient requirements for lactating women.
- Implemented nutrient target handling and nutritional contribution calculations.
- Addressed limitations in food composition coverage and nutrient feasibility.

### MILP Optimisation

- Designed and implemented the MILP-based food recommendation model using PuLP.
- Integrated the CBC solver for mixed-integer optimisation.
- Implemented food-selection and portion-quantity decisions.
- Developed nutritional, meal-structure, portion and food-variety constraints.
- Integrated dietary filtering for vegetarian and vegan diets, allergies, cuisine preferences and excluded foods.
- Implemented objective-function penalties and preference considerations.

### Web Application and Integration

- Developed the Flask backend connecting the GAM predictions, micronutrient module and MILP optimiser.
- Integrated the recommendation pipeline with the web interface.
- Implemented user-input processing and recommendation generation.
- Developed nutritional validation and recommendation reporting.

### System Evaluation

- Conducted functional testing across different dietary configurations and user restrictions.
- Performed controlled experiments to examine the effects of changes in lactation stage and maternal characteristics.
- Evaluated optimisation feasibility and nutritional target satisfaction.
- Identified limitations involving restrictive dietary configurations and nutrient availability.

### Collaborative Contribution

This was a collaborative Honours research project.

**Varushka Dayal** was responsible for the GAM development, training, evaluation and interpretability analysis.

Both components were integrated into the final recommendation system.

## Results and Evaluation

The GAM models were evaluated using a held-out 20% test dataset.

| Model | R² | Mean Absolute Error |
|---|---:|---:|
| Energy GAM | 0.9722 | 26.86 kcal/day |
| Protein GAM | 1.000 | 0.00 g/day |

The perfect protein result should be interpreted in context: the protein targets were deterministically generated from maternal weight, meaning the model learned to reproduce the underlying mathematical relationship. These metrics demonstrate agreement with the target-generation framework rather than independent clinical predictive validity.

The recommendation system was functionally evaluated using different scenarios, including:

- Unrestricted diets
- Vegetarian diets
- Vegan diets
- Food allergies
- Cuisine preferences
- Excluded foods
- Different maternal weights
- Different lactation stages

The controlled experiments demonstrated that changes in maternal characteristics and lactation-related inputs propagated through the recommendation pipeline and influenced the generated food selections and portion quantities.

The system also identified nutrients that could not be adequately supplied in certain restrictive scenarios, rather than incorrectly reporting that all nutritional targets had been satisfied.

## Technologies and Tools

| Category | Technologies |
|---|---|
| Programming Language | Python |
| Machine Learning | Generalised Additive Models (GAMs), pyGAM |
| Data Processing | pandas, NumPy |
| Mathematical Optimisation | PuLP, CBC Solver |
| Model Persistence | joblib, pickle |
| Backend | Flask, Flask-CORS |
| Frontend | HTML, CSS, JavaScript |
| Data Sources | DHS Program, USDA FoodData Central |
| Development | Jupyter Notebook, Google Colab, Visual Studio Code |

## Running the Project

### Download the Complete Source Code

The complete project setup is available for download here: https://drive.google.com/file/d/1nEMQ_ZYv_f44gzi1nAsJLlh8BPbzb8pc/view?usp=sharing 

To obtain the project:

1. Click the link.
2. Select **Download ZIP**.
4. Extract the downloaded archive to your computer.

The ZIP archive contains the complete project setup, including:

- Python application source code.
- Trained GAM models.
- Processed food and recipe catalogues.
- Data-preparation scripts.
- Jupyter notebooks.
- Supporting Python modules.
- Dependency configuration files.
- Frontend templates and static resources.

### Requirements

- Python 3.12
- pip
- A modern web browser
- Internet connection for initial package installation

### Installation

Extract the ZIP archive and open a terminal in the main project directory.

Create a virtual environment:

```bash
py -3.12 -m venv .venv
```

Activate it on Windows PowerShell:

```powershell
.\.venv\Scripts\Activate.ps1
```

Install the required dependencies:

```bash
python -m pip install --upgrade pip
python -m pip install -r requirements.txt
```

Start the Flask application:

```bash
python app.py
```

Open the following address in your browser:

```text
http://127.0.0.1:5000
```

**Important:** Keep the trained model files, food catalogues, `templates` folder and `static` folder in their supplied locations. Refer to the included setup files for additional installation and troubleshooting instructions.

## Limitations and Future Improvements

Although the system successfully demonstrates the integration of interpretable machine learning and mathematical optimisation, several limitations remain.

- Energy and protein training targets were derived from established nutritional equations rather than directly measured individual nutritional outcomes.
- High model performance does not establish clinical validity.
- Food composition datasets contain incomplete nutritional coverage for certain micronutrients.
- Highly restrictive dietary combinations can make it difficult to satisfy every nutritional constraint.
- Certain nutrients, particularly vitamin B12 and vitamin D in restrictive vegan meal plans, may require additional nutritional attention.
- The system has not yet undergone clinical validation with dietitians or real users.

Future improvements could include training with directly observed nutritional outcomes, expanding food composition coverage, improving recipe diversity and conducting user-based and clinical evaluations.

## Important Disclaimer

This project is an academic research prototype developed for an Honours research project. It is intended for research and educational purposes and does not replace professional nutritional advice, clinical assessment or consultation with a registered dietitian or healthcare professional.

## Project Information

**Project:** Stage-Based Nutrition Recommendation for Lactating Mothers Using an Interpretable Machine Learning Model  
**Institution:** University of KwaZulu-Natal  
**Year:** 2026

**Researchers:**
- Sasthi Naidoo
- Varushka Dayal

**Supervisors:**
- Dr Okuthe P. Kogeda
- Mr Zvinodashe Revesai

---

*This project demonstrates the integration of interpretable machine learning, nutritional data processing and mathematical optimisation to translate personalised nutritional requirements into practical food-based recommendations.*
