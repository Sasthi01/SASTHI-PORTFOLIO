// HELPERS

function escapeHTML(value) {
    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function formatNumber(value, decimals = 0) {
    const number = Number(value);

    if (!Number.isFinite(number)) {
        return "—";
    }

    return number.toLocaleString("en-ZA", {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });
}

function arrayValue(value) {
    if (value === null || value === undefined) {
        return [];
    }

    if (Array.isArray(value)) {
        return value;
    }

    if (typeof value === "string") {
        const trimmed = value.trim();

        if (
            !trimmed ||
            trimmed.toLowerCase() === "none"
        ) {
            return [];
        }

        return [trimmed];
    }

    return [value];
}

function uniqueValues(values) {
    return [
        ...new Set(
            arrayValue(values)
                .filter(Boolean)
                .map(value => String(value).trim())
                .filter(Boolean)
        )
    ];
}

function numberOrNull(value) {
    const number = Number(value);

    return Number.isFinite(number)
        ? number
        : null;
}

function decimalsForUnit(unit) {
    if (unit === "kcal") {
        return 0;
    }

    if (unit === "g") {
        return 1;
    }

    return 2;
}

function statusLabel(status) {
    switch (status) {
        case "met":
            return "✓ Met";

        case "below_target":
        case "attention_needed":
            return "⚠ Attention";

        case "not_verifiable":
            return "Not assessed";

        case "information_only":
        case "report_only":
            return "For information";

        case "not_applicable":
            return "Not applicable";

        default:
            return status
                ? String(status).replace(/_/g, " ")
                : "—";
    }
}

function statusClass(status) {
    if (status === "met") {
        return "status-good";
    }

    if (
        status === "below_target" ||
        status === "attention_needed"
    ) {
        return "status-warning";
    }

    return "status-neutral";
}

function targetWithinRange(actual, range) {
    const value = Number(actual);

    if (
        !Number.isFinite(value) ||
        !range
    ) {
        return false;
    }

    const minimum = Number(range.minimum);
    const maximum = Number(range.maximum);

    return (
        Number.isFinite(minimum) &&
        Number.isFinite(maximum) &&
        value >= minimum &&
        value <= maximum
    );
}

// FORM

function checkedValues(containerId) {
    return Array.from(
        document.querySelectorAll(
            `#${containerId} input[type="checkbox"]:checked`
        )
    ).map(checkbox => checkbox.value);
}

function collectFormData() {
    const getValue = id => {
        const element =
            document.getElementById(id);

        return element
            ? element.value
            : "";
    };

    return {
        age: getValue("age"),
        country: getValue("country"),
        children: getValue("children"),
        weight: getValue("weight"),
        height: getValue("height"),
        pregnant: getValue("pregnant"),
        breastfeeding: getValue("breastfeeding"),
        child_age_months: getValue("childAge"),
        diet: checkedValues("dietPreferences"),
        allergy: checkedValues("allergyPreferences"),
        cuisine: checkedValues("cuisinePreferences"),
        excludedFoods: checkedValues("excludedFoods")
    };
}

function validateForm(payload) {
    const errors = [];

    if (
        !payload.age ||
        Number(payload.age) < 18
    ) {
        errors.push(
            "Please enter a valid age of 18 years or older."
        );
    }

    if (
        !payload.weight ||
        Number(payload.weight) <= 0
    ) {
        errors.push(
            "Please enter a valid weight."
        );
    }

    if (
        !payload.height ||
        Number(payload.height) <= 0
    ) {
        errors.push(
            "Please enter a valid height."
        );
    }

    if (
        payload.children === "" ||
        Number(payload.children) < 0
    ) {
        errors.push(
            "Please enter a valid number of children."
        );
    }

    if (
        payload.child_age_months === "" ||
        Number(payload.child_age_months) < 0
    ) {
        errors.push(
            "Please enter the baby's age in months."
        );
    }

    if (!payload.country) {
        errors.push(
            "Please select your country."
        );
    }

    if (!payload.breastfeeding) {
        errors.push(
            "Please indicate whether you are breastfeeding."
        );
    }

    return errors;
}

// LOADING / ERRORS

function showLoading() {
    const loading =
        document.getElementById("loading");

    const output =
        document.getElementById("output");

    if (loading) {
        loading.style.display =
            "block";
    }

    if (output) {
        output.innerHTML = `
            <div class="nutrition-loading">

                <div class="nutrition-spinner"></div>

                <h3>
                    Preparing your personalised meal plan...
                </h3>

                <p>
                    We are creating a daily meal plan based on
                    your nutrition needs and food preferences.
                </p>

            </div>
        `;
    }
}

function hideLoading() {
    const loading =
        document.getElementById("loading");

    if (loading) {
        loading.style.display =
            "none";
    }
}

function showError(
    message,
    extra = []
) {
    const output =
        document.getElementById("output");

    if (!output) {
        return;
    }

    const extras =
        uniqueValues(extra);

    output.innerHTML = `
        <div class="nutrition-error">

            <div class="nutrition-error-icon">
                ⚠️
            </div>

            <div>

                <h3>
                    We couldn't create your plan
                </h3>

                <p>
                    ${escapeHTML(message)}
                </p>

                ${
                    extras.length
                        ? `
                            <ul>
                                ${
                                    extras
                                        .map(
                                            item =>
                                                `
                                                    <li>
                                                        ${escapeHTML(item)}
                                                    </li>
                                                `
                                        )
                                        .join("")
                                }
                            </ul>
                        `
                        : ""
                }

                <p>
                    Please review your selections and try again.
                </p>

            </div>

        </div>
    `;
}

// PERSONAL SUMMARY

function renderPersonalSummary(data) {
    const personal =
        data.personal_info || {};

    const stage =
        personal.lactation_stage ||
        data.lactation_stage ||
        "—";

    return `
        <section class="
            nutrition-section
            personal-summary
        ">

            <h2>
                Your personalised nutrition plan
            </h2>

            <p>
                Here is your daily meal plan based on your
                breastfeeding stage and the food preferences
                you selected.
            </p>

            <div class="personal-summary-grid">

                <div class="summary-card">

                    <span class="summary-card-label">
                        Breastfeeding stage
                    </span>

                    <strong>
                        ${escapeHTML(stage)}
                    </strong>

                </div>

                <div class="summary-card">

                    <span class="summary-card-label">
                        Country
                    </span>

                    <strong>
                        ${escapeHTML(
                            personal.country || "—"
                        )}
                    </strong>

                </div>

            </div>

        </section>
    `;
}

// PLAN READY CONFIRMATION

function renderPlanCheck(data) {
    const validation =
        data.validation || {};

    const accepted =
        validation.recommendation_accepted === true ||
        validation.validated === true;

    if (!accepted) {
        return "";
    }

    return `
        <section class="
            nutrition-section
            success-section
            plan-ready-section
        ">

            <div class="success-message">

                <span class="success-icon">
                    ✓
                </span>

                <div>

                    <strong>
                        Your plan is ready.
                    </strong>

                    <p>
                        Your meal plan meets the nutrition and
                        food-preference checks used by the
                        recommender.
                    </p>

                </div>

            </div>

        </section>
    `;
}

// DAILY ENERGY AND PROTEIN TARGETS

function renderMainTargets(data) {
    const targets =
        data.gam_targets || {};

    const actual =
        data.actual_nutrients || {};

    const energyTarget =
        numberOrNull(
            targets.energy_kcal
        );

    const proteinTarget =
        numberOrNull(
            targets.protein_g
        );

    const actualEnergy =
        numberOrNull(
            actual.energy_kcal
        );

    const actualProtein =
        numberOrNull(
            actual.protein_g
        );

    const energyMet =
        targetWithinRange(
            actualEnergy,
            targets.energy_allowed_range_kcal
        );

    const proteinMet =
        targetWithinRange(
            actualProtein,
            targets.protein_allowed_range_g
        );


    const targetCard = (
        label,
        unit,
        actualValue,
        targetValue,
        decimals,
        met
    ) => {

        return `
            <div class="target-card">

                <div class="target-card-header">

                    <span>
                        ${escapeHTML(label)}
                    </span>

                    <span class="${
                        met
                            ? "target-check"
                            : "target-warning"
                    }">
                        ${met ? "✓" : "!"}
                    </span>

                </div>

                <div class="target-main-value">

                    ${formatNumber(
                        actualValue,
                        decimals
                    )}

                    <span>
                        ${escapeHTML(unit)}
                    </span>

                </div>

                <div class="target-description">

                    Your daily target:

                    <strong>
                        ${formatNumber(
                            targetValue,
                            decimals
                        )}
                        ${escapeHTML(unit)}
                    </strong>

                </div>

                <p class="target-status-text">

                    ${
                        met
                            ? "✓ Your meal plan is within your personalised target range."
                            : "⚠ Your meal plan needs review for this target."
                    }

                </p>

            </div>
        `;
    };


    return `
        <section class="nutrition-section">

            <h2>
                Your daily nutrition targets
            </h2>

            <p>
                These energy and protein targets are
                personalised using the information you provided.
            </p>

            <div class="target-grid">

                ${targetCard(
                    "Energy",
                    "kcal",
                    actualEnergy,
                    energyTarget,
                    0,
                    energyMet
                )}

                ${targetCard(
                    "Protein",
                    "g",
                    actualProtein,
                    proteinTarget,
                    1,
                    proteinMet
                )}

            </div>

        </section>
    `;
}

// WHY THIS RECOMMENDATION?

function renderRecommendationExplanation(data) {
    const personal =
        data.personal_info || {};

    const stage =
        personal.lactation_stage ||
        data.lactation_stage ||
        "your current breastfeeding stage";

    return `
        <section class="
            nutrition-section
            recommendation-explanation-section
        ">

            <h2>
                Why this recommendation?
            </h2>

            <p>
                Your meal plan is personalised using information
                about you, your breastfeeding stage and the food
                preferences you selected.
            </p>

            <div class="explanation-grid">

                <div class="explanation-card">

                    <h3>
                        Breastfeeding stage
                    </h3>

                    <p>
                        Your baby's age places you in the
                        <strong>${escapeHTML(stage)}</strong>
                        stage. Your breastfeeding stage is used
                        when determining the nutrition guidance
                        for your plan.
                    </p>

                </div>

                <div class="explanation-card">

                    <h3>
                        Your nutrition needs
                    </h3>

                    <p>
                        An interpretable prediction model uses
                        your personal information and breastfeeding
                        stage to estimate your daily energy and
                        protein needs.
                    </p>

                </div>

                <div class="explanation-card">

                    <h3>
                        Your food preferences
                    </h3>

                    <p>
                        Your selected diet, allergies, cuisine
                        preferences and excluded foods are considered
                        when choosing suitable meals for your plan.
                    </p>

                </div>

            </div>

            <details class="model-explanation-details">

                <summary>
                    How does the prediction model work?
                </summary>

                <p>
                    The system uses a Generalized Additive Model
                    (GAM), an interpretable machine learning model,
                    to estimate personalised energy and protein
                    requirements. These targets are then used by
                    the meal-planning system to select foods and
                    portions that aim to meet your nutrition needs
                    while respecting your food preferences.
                </p>

            </details>

        </section>
    `;
}

// PREFERENCES

function renderFilterGroup(
    title,
    values,
    emptyMessage
) {
    const list =
        uniqueValues(values);

    if (!list.length) {
        return `
            <div class="filter-group">

                <strong>
                    ${escapeHTML(title)}
                </strong>

                <div class="filter-none">
                    ${escapeHTML(
                        emptyMessage
                    )}
                </div>

            </div>
        `;
    }

    return `
        <div class="filter-group">

            <strong>
                ${escapeHTML(title)}
            </strong>

            <div class="filter-tags">

                ${
                    list
                        .map(
                            item =>
                                `
                                    <span class="filter-tag">
                                        ${escapeHTML(item)}
                                    </span>
                                `
                        )
                        .join("")
                }

            </div>

        </div>
    `;
}

function renderFilters(data) {
    const filters =
        data.filters_applied || {};

    return `
        <section class="
            nutrition-section
            filters-section
        ">

            <h2>
                Your preferences
            </h2>

            ${renderFilterGroup(
                "Diet",
                filters.diet,
                "No specific diet selected"
            )}

            ${renderFilterGroup(
                "Allergies",
                filters.allergies,
                "No allergies selected"
            )}

            ${renderFilterGroup(
                "Cuisine",
                filters.cuisine,
                "No specific cuisine selected"
            )}

            ${renderFilterGroup(
                "Foods excluded",
                filters.excluded_foods,
                "No foods excluded"
            )}

        </section>
    `;
}

// MEAL PLAN

function servingNote(food) {
    const practicalText =
        food.display_serving ||
        food.portion_text ||
        food.serving_description;

    if (practicalText) {
        return escapeHTML(
            practicalText
        );
    }

    const grams =
        numberOrNull(
            food.grams
        );

    if (grams !== null) {
        return (
            `About ${formatNumber(grams, 0)} g`
        );
    }

    return "";
}


function renderMealItem(food) {
    const name =
        food.food_name ||
        food.name ||
        food.food_item ||
        "Food";

    const portion =
        servingNote(food);

    return `
        <div class="food-item">

            <div class="food-item-main">

                <strong class="food-name">
                    ${escapeHTML(name)}
                </strong>

                ${
                    portion
                        ? `
                            <div class="food-amount">
                                ${portion}
                            </div>
                        `
                        : ""
                }

            </div>

        </div>
    `;
}

function renderRecommendations(data) {
    const recommendations =
        data.meal_plan ||
        data.recommendations ||
        {};

    const preferredOrder = [
        "Breakfast",
        "Lunch",
        "Snack",
        "Supper",
        "Dessert"
    ];

    const groups = [];

    for (const name of preferredOrder) {

        if (
            Array.isArray(
                recommendations[name]
            ) &&
            recommendations[name].length
        ) {
            groups.push([
                name,
                recommendations[name]
            ]);
        }
    }

    for (
        const [
            name,
            foods
        ] of Object.entries(
            recommendations
        )
    ) {

        if (
            preferredOrder.includes(name) ||
            !Array.isArray(foods) ||
            !foods.length
        ) {
            continue;
        }

        groups.push([
            name,
            foods
        ]);
    }

    if (!groups.length) {
        return `
            <section class="nutrition-section">

                <h2>
                    Your recommended meals
                </h2>

                <p>
                    No meal recommendations were returned.
                </p>

            </section>
        `;
    }

    const body =
        groups
            .map(
                ([group, foods]) =>
                    `
                        <div class="food-group">

                            <h4>
                                ${escapeHTML(group)}
                            </h4>

                            <div class="food-list">

                                ${
                                    foods
                                        .map(
                                            renderMealItem
                                        )
                                        .join("")
                                }

                            </div>

                        </div>
                    `
            )
            .join("");


    return `
        <section class="
            nutrition-section
            meal-plan-section
        ">

            <h2>
                Your recommended meals
            </h2>

            <p>
                Use the household portions below as a practical
                guide for your meals.
            </p>

            ${body}

        </section>
    `;
}

// NUTRIENT HELPERS

function renderNutrientCell(
    value,
    unit,
    decimals
) {
    if (
        value === null ||
        value === undefined ||
        !Number.isFinite(
            Number(value)
        )
    ) {
        return "—";
    }

    return (
        `${formatNumber(value, decimals)} ` +
        `${escapeHTML(unit || "")}`
    );
}

function normalizedNutrientLabel(row) {
    return String(
        row.label || ""
    )
        .trim()
        .toLowerCase();
}

// VITAMINS AND MINERALS

function renderNutrientTable(data) {
    const rows =
        Array.isArray(
            data.nutrient_report
        )
            ? data.nutrient_report
            : [];

    if (!rows.length) {
        return "";
    }

    const excludedLabels =
        new Set([
            "energy",
            "protein",
            "iodine",
            "fat",
            "carbohydrates",
            "carbohydrate",
            "fibre",
            "fiber",
            "sodium"
        ]);

    const visibleRows =
        rows.filter(
            row =>
                !excludedLabels.has(
                    normalizedNutrientLabel(row)
                )
        );

    if (!visibleRows.length) {
        return "";
    }

    const body =
        visibleRows
            .map(
                row => {

                    const unit =
                        row.unit || "";

                    const decimals =
                        decimalsForUnit(unit);

                    const notAssessed =
                        row.status ===
                        "not_verifiable";

                    const provided =
                        notAssessed
                            ? "Not assessed"
                            : renderNutrientCell(
                                row.actual,
                                unit,
                                decimals
                            );

                    const target =
                        row.target === null ||
                        row.target === undefined
                            ? "—"
                            : renderNutrientCell(
                                row.target,
                                unit,
                                decimals
                            );


                    return `
                        <tr>

                            <td>

                                <strong>
                                    ${escapeHTML(
                                        row.label ||
                                        "Nutrient"
                                    )}
                                </strong>

                            </td>

                            <td>
                                ${provided}
                            </td>

                            <td>
                                ${target}
                            </td>

                            <td>

                                <span class="
                                    nutrient-status
                                    ${statusClass(
                                        row.status
                                    )}
                                ">

                                    ${escapeHTML(
                                        statusLabel(
                                            row.status
                                        )
                                    )}

                                </span>

                            </td>

                        </tr>
                    `;
                }
            )
            .join("");

    return `
        <section class="
            nutrition-section
            vitamins-minerals-section
        ">

            <h2>
                Vitamins and minerals
            </h2>

            <p>
                This table shows how your meal plan compares
                with the daily nutrition targets used for
                breastfeeding women.
            </p>

            <div class="nutrition-table-container">

                <table class="nutrition-table">

                    <thead>

                        <tr>

                            <th>
                                Nutrient
                            </th>

                            <th>
                                Your plan
                            </th>

                            <th>
                                Daily target
                            </th>

                            <th>
                                Status
                            </th>

                        </tr>

                    </thead>

                    <tbody>
                        ${body}
                    </tbody>

                </table>

            </div>

            <p class="nutrient-table-note">
                These targets are nutrition planning guides
                and do not diagnose a nutrient deficiency.
            </p>

        </section>
    `;
}

// OTHER NUTRITION INFORMATION

function renderOtherNutritionInformation(data) {
    const rows =
        Array.isArray(
            data.nutrient_report
        )
            ? data.nutrient_report
            : [];

    if (!rows.length) {
        return "";
    }

    const wantedLabels = [
        "fat",
        "carbohydrates",
        "fibre",
        "sodium"
    ];

    const aliases = {
        fat: [
            "fat"
        ],

        carbohydrates: [
            "carbohydrates",
            "carbohydrate"
        ],

        fibre: [
            "fibre",
            "fiber"
        ],

        sodium: [
            "sodium"
        ]
    };

    const displayNames = {
        fat: "Fat",
        carbohydrates: "Carbohydrates",
        fibre: "Fibre",
        sodium: "Sodium"
    };

    const nutrientMap = {};

    for (const key of wantedLabels) {

        nutrientMap[key] =
            rows.find(
                row =>
                    aliases[key].includes(
                        normalizedNutrientLabel(row)
                    )
            ) || null;
    }

    const available =
        wantedLabels.filter(
            key =>
                nutrientMap[key] &&
                Number.isFinite(
                    Number(
                        nutrientMap[key].actual
                    )
                )
        );

    if (!available.length) {
        return "";
    }

    const cards =
        available
            .map(
                key => {

                    const row =
                        nutrientMap[key];

                    const unit =
                        row.unit || "";

                    const decimals =
                        key === "sodium"
                            ? 0
                            : decimalsForUnit(unit);

                    return `
                        <div class="
                            other-nutrition-card
                        ">

                            <span class="
                                other-nutrition-label
                            ">
                                ${escapeHTML(
                                    displayNames[key]
                                )}
                            </span>

                            <strong class="
                                other-nutrition-value
                            ">
                                ${formatNumber(
                                    row.actual,
                                    decimals
                                )}
                                ${escapeHTML(unit)}
                            </strong>

                        </div>
                    `;
                }
            )
            .join("");

    return `
        <section class="
            nutrition-section
            other-nutrition-section
        ">

            <h2>
                Other nutrition information
            </h2>

            <div class="
                other-nutrition-grid
            ">
                ${cards}
            </div>

            <p class="
                nutrient-table-note
                other-nutrition-note
            ">
                These values provide additional information
                about the nutritional composition of your
                daily meal plan.
            </p>

        </section>
    `;
}

// NUTRIENTS NEEDING ATTENTION

function renderAdditionalAttention(data) {
    const items =
        Array.isArray(
            data.additional_attention
        )
            ? data.additional_attention
            : [];

    if (!items.length) {
        return "";
    }

    const body =
        items
            .map(
                item => `
                    <div class="attention-card">

                        <h3>
                            ${escapeHTML(
                                item.label ||
                                "Nutrient"
                            )}
                        </h3>

                        <p>
                            ${escapeHTML(
                                item.message || ""
                            )}
                        </p>

                        ${
                            Number.isFinite(
                                Number(item.actual)
                            ) &&
                            Number.isFinite(
                                Number(item.target)
                            )
                                ? `
                                    <p class="attention-values">

                                        Your plan provides

                                        <strong>
                                            ${formatNumber(
                                                item.actual,
                                                decimalsForUnit(
                                                    item.unit || ""
                                                )
                                            )}
                                            ${escapeHTML(
                                                item.unit || ""
                                            )}
                                        </strong>

                                        compared with a daily
                                        target of

                                        <strong>
                                            ${formatNumber(
                                                item.target,
                                                decimalsForUnit(
                                                    item.unit || ""
                                                )
                                            )}
                                            ${escapeHTML(
                                                item.unit || ""
                                            )}
                                        </strong>.

                                    </p>
                                `
                                : ""
                        }

                    </div>
                `
            )
            .join("");

    return `
        <section class="
            nutrition-section
            warning-section
            additional-attention-section
        ">

            <h2>
                Nutrients that need attention
            </h2>

            <p>
                Your meal plan is lower than the daily target
                for the nutrient${items.length > 1 ? "s" : ""}
                below.
            </p>

            ${body}

            <p>
                Fortified foods may help with some nutrients.
                Before starting or changing a supplement,
                speak to a doctor, dietitian, nurse or other
                qualified healthcare professional.
            </p>

        </section>
    `;
}

// IMPORTANT INFORMATION

function renderDisclaimer() {
    return `
        <section class="
            nutrition-section
            disclaimer-section
        ">

            <h3>
                Important information
            </h3>

            <p>

                <strong>
                    Iodine is not shown
                </strong>

                because the food dataset does not contain
                enough reliable iodine information to assess
                your intake.

            </p>

            <p>
                This meal plan provides general nutrition
                guidance and does not replace personalised
                advice from a doctor, dietitian, nurse or
                other qualified healthcare professional.
            </p>

            <p>
                Your individual needs may differ, especially
                if you have a medical condition, take
                medication, have been advised to follow a
                special diet, or have concerns about your
                own or your baby's nutrition.
            </p>

        </section>
    `;
}

// REPORT CONTROLS

function renderReportControls() {
    return `
        <div class="
            report-controls
            no-print
        ">

            <button
                type="button"
                id="printReportButton"
                class="report-button"
            >
                Print / Save Report
            </button>

            <button
                type="button"
                id="newPlanButton"
                class="report-button secondary"
            >
                Create Another Plan
            </button>

        </div>
    `;
}

function attachReportControls() {
    const printButton =
        document.getElementById(
            "printReportButton"
        );

    const newPlanButton =
        document.getElementById(
            "newPlanButton"
        );

    if (printButton) {

        printButton.addEventListener(
            "click",
            () => window.print()
        );
    }

    if (newPlanButton) {

        newPlanButton.addEventListener(
            "click",
            () => {

                const form =
                    document.getElementById(
                        "nutritionForm"
                    );

                const output =
                    document.getElementById(
                        "output"
                    );

                if (form) {
                    form.reset();
                }

                if (output) {
                    output.innerHTML = "";
                }

                window.scrollTo({
                    top: 0,
                    behavior: "smooth"
                });
            }
        );
    }
}

// PRINT STYLES

function ensureReportPrintStyles() {
    if (
        document.getElementById(
            "motherReportPrintStyles"
        )
    ) {
        return;
    }

    const style =
        document.createElement(
            "style"
        );

    style.id =
        "motherReportPrintStyles";

    style.textContent = `

        /* --------------------------------------------
           WHY THIS RECOMMENDATION?
        --------------------------------------------- */

        .recommendation-explanation-section {
            margin-top: 1.5rem;
        }

        .explanation-grid {
            display: grid;
            grid-template-columns:
                repeat(3, minmax(0, 1fr));
            gap: 12px;
            margin-top: 16px;
        }

        .explanation-card {
            padding: 16px;
            border: 1px solid #e2e2e2;
            border-radius: 10px;
            box-sizing: border-box;
            background: #ffffff;
        }

        .explanation-card h3 {
            margin-top: 0;
            margin-bottom: 8px;
            font-size: 1rem;
            color: #ff5e62;
        }

        .explanation-card p {
            margin-bottom: 0;
            line-height: 1.6;
        }

        .model-explanation-details {
            margin-top: 16px;
            padding: 14px 16px;
            border: 1px solid #e2e2e2;
            border-radius: 10px;
            background: #fff8f8;
        }

        .model-explanation-details summary {
            cursor: pointer;
            font-weight: 600;
            color: #ff5e62;
        }

        .model-explanation-details p {
            margin-bottom: 0;
            line-height: 1.6;
        }


        /* --------------------------------------------
           OTHER NUTRITION INFORMATION
        --------------------------------------------- */

        .other-nutrition-grid {
            display: grid;
            grid-template-columns:
                repeat(4, minmax(0, 1fr));
            gap: 12px;
            margin-top: 16px;
        }

        .other-nutrition-card {
            padding: 16px 12px;
            border: 1px solid #e2e2e2;
            border-radius: 10px;
            text-align: center;
            box-sizing: border-box;
        }

        .other-nutrition-label {
            display: block;
            margin-bottom: 6px;
            font-size: 0.92rem;
        }

        .other-nutrition-value {
            display: block;
            font-size: 1.1rem;
        }


        /* --------------------------------------------
           PRINT
        --------------------------------------------- */

        @media print {

            .no-print,
            .report-controls,
            #printReportButton,
            #newPlanButton {
                display: none !important;
            }

            .plan-ready-section,
            .target-card,
            .summary-card,
            .recommendation-explanation-section,
            .explanation-card,
            .model-explanation-details,
            .food-group,
            .food-item,
            .attention-card,
            .other-nutrition-section,
            .other-nutrition-card,
            .disclaimer-section {
                break-inside: avoid;
                page-break-inside: avoid;
            }

            .nutrition-table tr {
                break-inside: avoid;
                page-break-inside: avoid;
            }

            .nutrition-section h2,
            .nutrition-section h3,
            .nutrition-section h4 {
                break-after: avoid;
                page-break-after: avoid;
            }

            .other-nutrition-grid {
                grid-template-columns:
                    repeat(4, 1fr);
            }

        }

        /* --------------------------------------------
           SMALLER SCREENS
        --------------------------------------------- */

        @media screen and (max-width: 700px) {

            .explanation-grid {
                grid-template-columns: 1fr;
            }

            .other-nutrition-grid {
                grid-template-columns:
                    repeat(2, minmax(0, 1fr));
            }

        }

    `;

    document.head.appendChild(
        style
    );
}

// COMPLETE MOTHER-FACING REPORT

function renderResults(data) {
    const output =
        document.getElementById(
            "output"
        );

    if (!output) {
        return;
    }

    ensureReportPrintStyles();

    output.innerHTML = `
        <div class="nutrition-report">

            ${renderPersonalSummary(data)}

            ${renderPlanCheck(data)}

            ${renderMainTargets(data)}

            ${renderRecommendationExplanation(data)}

            ${renderFilters(data)}

            ${renderRecommendations(data)}

            ${renderNutrientTable(data)}

            ${renderOtherNutritionInformation(data)}

            ${renderAdditionalAttention(data)}

            ${renderDisclaimer()}

            ${renderReportControls()}

        </div>
    `;

    attachReportControls();
}

// SUBMIT

async function submitNutritionForm(event) {
    event.preventDefault();


    const payload =
        collectFormData();


    const errors =
        validateForm(payload);


    if (errors.length) {

        showError(
            errors.join(" ")
        );

        return;
    }


    showLoading();


    try {

        const response =
            await fetch(
                "/optimize",
                {
                    method: "POST",

                    headers: {
                        "Content-Type":
                            "application/json"
                    },

                    body:
                        JSON.stringify(
                            payload
                        )
                }
            );


        let result;


        try {

            result =
                await response.json();

        } catch (error) {

            throw new Error(
                "The server returned an invalid response."
            );
        }


        if (
            !response.ok ||
            result.error
        ) {

            const extras = [

                ...arrayValue(
                    result.validation_errors
                ),

                ...arrayValue(
                    result.filter_warnings
                ),

                ...arrayValue(
                    result.warnings
                )

            ];


            showError(
                result.error ||
                `Server error (${response.status})`,
                extras
            );


            return;
        }


        renderResults(
            result
        );


        const output =
            document.getElementById(
                "output"
            );


        if (output) {

            setTimeout(
                () => {

                    output.scrollIntoView({
                        behavior: "smooth",
                        block: "start"
                    });

                },
                100
            );
        }


    } catch (error) {

        console.error(
            "Nutrition recommendation error:",
            error
        );


        showError(
            error.message ||
            "Something went wrong while creating your nutrition plan."
        );


    } finally {

        hideLoading();
    }
}

// INITIALISATION

document.addEventListener(
    "DOMContentLoaded",
    () => {

        ensureReportPrintStyles();


        const form =
            document.getElementById(
                "nutritionForm"
            );


        if (!form) {

            console.error(
                "Could not find #nutritionForm."
            );

            return;
        }


        form.addEventListener(
            "submit",
            submitNutritionForm
        );


        console.log(
            "USDA maternal nutrition recommender — mother-facing report v7.13 ready."
        );
    }
);