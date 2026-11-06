# Cardiovascular Disease Risk Prediction Using Machine Learning and Explainable AI

## Project Overview

This project focuses on developing a machine learning system to predict the presence of cardiovascular disease (CVD) using demographic, physiological, and lifestyle-related patient information.

The project uses classical machine learning algorithms, ensemble learning, and Explainable Artificial Intelligence (XAI) to investigate predictive performance and understand the factors influencing model predictions.

The main objective was to develop an end-to-end machine learning pipeline, compare individual classification models against an ensemble model, and investigate whether combining models could improve cardiovascular disease prediction.

## Dataset

The project uses the publicly available **Cardiovascular Disease Dataset** from Kaggle.

- Approximately 70,000 original patient records.
- 13 original variables.
- Binary classification problem.
- Target variable: `cardio`
  - `0` – No cardiovascular disease.
  - `1` – Cardiovascular disease.

The dataset contains demographic, physiological, and lifestyle information, including age, gender, blood pressure, cholesterol, glucose, smoking, alcohol consumption, and physical activity.

[Dataset – Kaggle](https://www.kaggle.com/datasets/sulianova/cardiovascular-disease-dataset)

## Phase 3 Project Objectives

The final phase focused on completing and evaluating the machine learning pipeline through:

- Developing and evaluating a Decision Tree classifier.
- Developing and evaluating a Support Vector Machine (SVM).
- Implementing and comparing ensemble learning configurations.
- Performing hyperparameter optimization using cross-validation.
- Applying SHAP to explain global feature importance and individual predictions.
- Comparing feature importance across the three models.
- Performing statistical comparisons of model performance.
- Evaluating the final models using appropriate classification metrics and visualisations.

## Methodology

### 1. Data Preprocessing and Feature Engineering

The dataset underwent preprocessing to improve data quality and consistency. This included checking for missing values, duplicate records, invalid measurements, and physiologically implausible observations.

Feature engineering was used to derive additional information from the original variables.

Examples of engineered features include:

- **Age in Years:** Converted age from days into years.
- **Body Mass Index (BMI):** Calculated using weight and height.
- **Pulse Pressure:** Difference between systolic and diastolic blood pressure.
- **Mean Arterial Pressure (MAP):** Derived from blood pressure measurements.
- **Hypertension Indicator:** Created using systolic and diastolic blood pressure thresholds.
- **Lifestyle Risk Score:** Combined selected lifestyle-related factors.
- **Age Category and BMI Category:** Created to represent broader patient groups.

Feature selection was then performed to identify the variables used in the final models.

The final selected feature set contained 10 variables:

`age_years`, `gender`, `ap_hi`, `ap_lo`, `cholesterol`, `gluc`, `smoke`, `active`, `bmi`, and `hypertension`.

### 2. Model Development

Three classification models were evaluated:

| Model | Description |
|---|---|
| Decision Tree | Tree-based classification model |
| Support Vector Machine (SVM) | Classification model using an optimized kernel |
| Soft Voting Ensemble | Combines the predictions of the Decision Tree and SVM |

Hyperparameter selection and model comparisons used stratified 5-fold cross-validation.

Four ensemble configurations were evaluated:

- Equal-weight soft voting.
- Decision Tree-weighted soft voting.
- SVM-weighted soft voting.
- Stacking with a Logistic Regression meta-learner.

Equal-weight soft voting achieved the highest mean cross-validation accuracy and was selected as the final ensemble configuration.

### 3. Model Evaluation

The final models were evaluated on a held-out test set using:

- Accuracy
- Precision
- Recall
- Specificity
- F1-score
- ROC-AUC
- Matthews Correlation Coefficient (MCC)
- Cohen's Kappa

Additional visualisations included confusion matrices, ROC curves, Precision-Recall curves, and learning curves.

### 4. Explainable Artificial Intelligence (SHAP)

SHAP (SHapley Additive exPlanations) was implemented to understand how individual features influenced the predictions made by the Decision Tree, SVM, and Soft Voting Ensemble.

The SHAP analysis included:

- Global SHAP summary plots.
- Global SHAP bar plots.
- SHAP dependence analysis.
- Local explanations using waterfall plots.
- Cross-model comparison of feature importance.

A background sample of 100 training observations and a sample of 300 held-out test observations were used for the explanations.

The analysis identified systolic blood pressure (`ap_hi`), hypertension, age, and cholesterol as important predictive features.

The results also demonstrated how individual feature contributions could increase or decrease a patient's predicted CVD probability.

## Final Model Results

The final held-out test-set results were:

| Metric | Decision Tree | SVM | Soft Voting Ensemble |
|---|---:|---:|---:|
| Accuracy | 73.71% | 73.66% | **73.89%** |
| Precision | 76.79% | 77.37% | **77.47%** |
| Recall | **67.17%** | 66.08% | 66.59% |
| Specificity | 80.12% | **81.08%** | 81.03% |
| F1-score | **71.66%** | 71.28% | 71.62% |
| ROC-AUC | 0.7962 | 0.7904 | **0.8015** |
| MCC | 0.4772 | 0.4773 | **0.4816** |
| Cohen's Kappa | 0.4735 | 0.4723 | **0.4770** |

The Soft Voting Ensemble achieved the highest test accuracy, precision, ROC-AUC, MCC, and Cohen's Kappa among the three models. However, the Decision Tree achieved the highest recall and F1-score, while the SVM achieved the highest specificity.

Statistical testing using cross-validation results indicated that the ensemble's improvement over the SVM was statistically significant, while its improvement over the Decision Tree was not statistically significant.

## My Contribution – Explainable AI (SHAP)

My primary responsibility during Phase 3 was the implementation and analysis of the Explainable Artificial Intelligence component.

I developed the SHAP analysis for all three final models: the Decision Tree, SVM, and Soft Voting Ensemble.

My contributions included:

- Setting up the SHAP environment and preparing reproducible background and test samples.
- Developing prediction functions compatible with SHAP for all three models.
- Implementing model-agnostic permutation-based SHAP explanations.
- Generating global SHAP summary plots to investigate feature influence and direction.
- Generating global SHAP bar plots to measure mean absolute feature importance.
- Creating a cross-model comparison of normalized SHAP feature importance.
- Generating a SHAP dependence plot to investigate the relationship between systolic blood pressure and model predictions.
- Producing local waterfall explanations for correctly classified CVD and non-CVD patients.
- Comparing the same patient's SHAP explanations across the Decision Tree, SVM, and Ensemble.
- Interpreting the findings in relation to clinically relevant cardiovascular risk factors.

The analysis showed that systolic blood pressure was the most influential feature for the Decision Tree and Ensemble, while hypertension ranked highest for the SVM.

For the Ensemble, systolic blood pressure accounted for 44.94% of normalized relative SHAP importance, followed by hypertension (16.87%), age (16.30%), and cholesterol (11.54%).

This work helped demonstrate that the models could provide explanations for their predictions rather than functioning solely as black-box classifiers.

## Technologies and Libraries

The project was developed using Python and Google Colab.

| Technology / Library | Purpose |
|---|---|
| Python | Main programming language |
| Google Colab | Development and execution environment |
| Pandas | Data manipulation and analysis |
| NumPy | Numerical operations |
| Matplotlib | Data visualisation |
| Seaborn | Statistical visualisation |
| Scikit-learn | Machine learning, preprocessing, and evaluation |
| SHAP | Explainable AI and model interpretation |
| Joblib | Model and SHAP result caching |

## Limitations and Future Improvements

Although the models demonstrated the potential of machine learning for cardiovascular disease prediction, several limitations remain:

- The models were evaluated using one dataset, limiting conclusions about generalisation to other populations.
- Some patients with cardiovascular disease were incorrectly classified.
- SHAP explains model behaviour but does not establish causal relationships.
- Further external validation is required before considering real-world clinical applications.

Future improvements could include testing on independent datasets, investigating additional machine learning algorithms, incorporating more clinical information, and exploring improved classification thresholds.

## Conclusion

This project demonstrates the application of classical machine learning, ensemble learning, and explainable AI to cardiovascular disease prediction.

The comparison of the Decision Tree, SVM, and Soft Voting Ensemble provided insight into their predictive strengths and limitations. SHAP further improved model transparency by identifying influential features and explaining individual predictions.

Overall, the project highlights the importance of combining predictive performance with explainability when developing machine learning systems for healthcare-related applications.

**Disclaimer:** This project is an academic machine learning study and is not intended to provide medical diagnoses or replace professional medical assessment.
