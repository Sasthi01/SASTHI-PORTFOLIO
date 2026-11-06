# app.py
from __future__ import annotations
import math
import os
import re
from typing import Dict, List, Optional, Tuple
import joblib
import pandas as pd
import pulp
from flask import Flask, jsonify, render_template, request

# APP / PATHS

app = Flask(__name__)
BASE_DIR = os.path.dirname(os.path.abspath(__file__))

FOOD_FILE = os.path.join(BASE_DIR, "usda_lp_food_catalogue.csv",)
RECIPE_FILE = os.path.join(BASE_DIR, "recipe_catalogue_final.csv",)
PROTEIN_MODEL_FILE = os.path.join(BASE_DIR, "protein_gam_model.pkl",)
ENERGY_MODEL_FILE = os.path.join(BASE_DIR, "energy_gam_model.pkl",)

# MAIN OPTIMISATION SETTINGS

ENERGY_MIN_RATIO = 0.98
ENERGY_MAX_RATIO = 1.02

# CBC may return values a tiny fraction below or above a hard boundary
# because of floating-point feasibility tolerances. These tolerances are
# used only by the post-solve validation and do not alter the LP constraints.
ENERGY_VALIDATION_TOLERANCE_KCAL = 0.5
PROTEIN_MIN_RATIO = 0.95
PROTEIN_MAX_RATIO = 1.05
PROTEIN_VALIDATION_TOLERANCE_G = 0.05
MAX_SUPPORTING_ITEMS = 4

MAX_BREAKFAST_CANDIDATES = 90
MAX_LUNCH_CANDIDATES = 140
MAX_SNACK_CANDIDATES = 90
MAX_SUPPER_CANDIDATES = 140
MAX_DESSERT_CANDIDATES = 60
MAX_SIDE_CANDIDATES = 220
SOLVER_TIME_LIMIT_SECONDS = 45

# Meal-level energy ranges as fractions of the GAM energy target.
# These are engineering controls for meal realism, not clinical rules.
MEAL_ENERGY_BANDS = {
    "breakfast": (0.12, 0.35),
    "lunch": (0.18, 0.42),
    "snack": (0.03, 0.20),
    "supper": (0.18, 0.42),
    "dessert": (0.00, 0.15),
}

# Supporting foods are limited to practical side/component groups.
SIDE_GROUPS = {
    "Vegetables",
    "Fruit",
    "Dairy",
    "Grains & Staples",
    "Legumes & Pulses",
    "Nuts & Seeds",
    "Eggs",
}

# Default serving size if USDA does not provide one.
FALLBACK_SERVINGS_G = {
    "breakfast": 180.0,
    "lunch": 300.0,
    "snack": 100.0,
    "supper": 300.0,
    "dessert": 100.0,
    "side": 100.0,
}

GROUP_FALLBACK_SERVINGS_G = {
    "Vegetables": 100.0,
    "Fruit": 120.0,
    "Dairy": 200.0,
    "Grains & Staples": 100.0,
    "Legumes & Pulses": 150.0,
    "Nuts & Seeds": 30.0,
    "Eggs": 50.0,
    "Meat & Poultry": 120.0,
    "Fish & Seafood": 120.0,
    "Oils & Fats": 10.0,
}

# Reporting-only practical portion references.
# These do NOT change the nutrient calculation or the LP gram bounds.
# They are used only when a USDA household portion is too small to be
# meaningful for presentation, such as "1 leaf" of kale.
REPORTING_FALLBACK_SERVINGS_G = {
    "Vegetables": 75.0,
}

MIN_USDA_VEGETABLE_REPORTING_PORTION_G = 25.0

# Per-selection serving ranges.
MEAL_SERVING_RANGES = {
    "breakfast": (0.75, 1.50),
    "lunch": (0.75, 1.50),
    "snack": (0.50, 1.50),
    "supper": (0.75, 1.50),
    "dessert": (0.50, 1.00),
    "side": (0.50, 2.00),
}

# Extra absolute gram limits for supporting foods.
SIDE_GROUP_MAX_GRAMS = {
    "Vegetables": 300.0,
    "Fruit": 300.0,
    "Dairy": 350.0,
    "Grains & Staples": 250.0,
    "Legumes & Pulses": 250.0,
    "Nuts & Seeds": 60.0,
    "Eggs": 150.0,
}

# More realistic supporting-food portion ranges.
# They are not medical intake targets.
SIDE_SUBTYPE_GRAM_BOUNDS = {
    "fresh_fruit": (50.0, 250.0),
    "dried_fruit": (15.0, 40.0),
    "nuts_seeds": (15.0, 40.0),
    "ordinary_dairy": (100.0, 300.0),
    "cooked_vegetable": (75.0, 200.0),
    "cooked_mushroom": (50.0, 150.0),
    "raw_leafy_vegetable": (30.0, 100.0),
    "other_vegetable": (50.0, 200.0),
    "grain_staple": (50.0, 200.0),
    "legume": (75.0, 200.0),
    "egg": (40.0, 150.0),
}

# FOOD-QUALITY / MEAL-REALISM RULES

PROCESSED_VEGETABLE_TERMS = {
    "tot",
    "tots",
    "french fries",
    "fries",
    "fried",
    "chip",
    "chips",
    "hash brown",
    "hash browns",
    "croquette",
    "croquettes",
    "puff",
    "puffs",
    "vegetable snack",
}

CONVENIENCE_TERMS = {
    "pot pie",
    "fried",
    "fries",
    "tots",
    "hash brown",
    "chips",
    "pizza",
    "burger",
    "fast food",
    "breaded",
    "battered",
}

# Foods that should never be used as a main meal item by themselves.
# They can still remain available as supporting components when sensible.
MAIN_COMPONENT_ONLY_TERMS = {
    "syrup",
    "pancake syrup",
    "maple syrup",
    "honey",
    "jam",
    "jelly",
    "marmalade",
    "butter",
    "margarine",
    "spread",
    "sauce",
    "gravy",
    "dressing",
    "topping",
    "cream",
    "creamer",
    "taco shell",
    "taco shells",
    "tortilla shell",
    "tortilla shells",
    "wrap shell",
    "bread crumbs",
    "cracker crumbs",
    "seasoning",
    "condiment",
}

# Supplement-like or powder products are excluded completely from the
# recommendation catalogue at runtime.
SUPPLEMENT_LIKE_TERMS = {
    "nutritional powder",
    "nutrition powder",
    "protein powder",
    "protein supplement",
    "meal replacement",
    "shake powder",
    "muscle milk",
    "ensure",
    "boost",
    "supplement drink",
    "nutritional supplement",
    "nutritional drink",
    "nutrition drink",
    "nutritional shake",
    "nutrition shake",
}

# Concentrated ingredient powders that should not be recommended
# as standalone supporting foods.
CONCENTRATED_INGREDIENT_TERMS = {
    "milk, dry",
    "dry milk",
    "milk powder",
    "powdered milk",
    "nonfat dry milk",
    "skim milk powder",
    "whole milk powder",
    "buttermilk powder",
}

DRIED_FRUIT_TERMS = {
    "dried",
    "dehydrated",
    "raisins",
    "raisin",
    "prunes",
    "prune",
    "dates",
    "date,",
    "dried apricot",
    "dried cranberry",
    "dried blueberries",
    "dried cherries",
    "dried mango",
    "dried banana",
}

RAW_LEAFY_VEGETABLE_TERMS = {
    "raw",
    "spinach",
    "lettuce",
    "kale",
    "dandelion greens",
    "collard greens",
    "mustard greens",
    "turnip greens",
    "beet greens",
    "arugula",
    "rocket",
    "watercress",
}

COOKED_VEGETABLE_TERMS = {
    "cooked",
    "boiled",
    "steamed",
    "baked",
    "roasted",
    "braised",
    "as ingredient",
}

COMMON_DAIRY_TERMS = {
    "milk",
    "yogurt",
    "yoghurt",
    "kefir",
    "cottage cheese",
}

# Daily variety and presentation-quality rules.
PROCESSED_BAR_TERMS = {
    "granola bar",
    "cereal bar",
    "nutrition bar",
    "breakfast bar",
    "snack bar",
    "energy bar",
    "protein bar",
}

AS_INGREDIENT_TERMS = {
    "as ingredient",
    "used as ingredient",
}

MAIN_DISH_FAMILY_RULES = [
    ("sandwich", {"sandwich", "burger", "hot dog", "frankfurter"}),
    ("curry", {"curry"}),
    ("stew", {"stew"}),
    ("pasta", {"pasta", "spaghetti", "lasagna", "lasagne", "macaroni"}),
    ("rice_mixed", {"biryani", "fried rice", "rice mixed", "rice dish"}),
    ("soup", {"soup", "chowder"}),
    ("pizza", {"pizza"}),
    ("mexican_main", {"taco", "burrito", "quesadilla", "enchilada", "fajita"}),
    ("egg_main", {"omelet", "omelette"}),
    ("legume_main", {"lentil", "bean", "chickpea"}),
    ("fish_main", {"fish", "salmon", "tuna", "hake", "cod", "tilapia"}),
    ("chicken_main", {"chicken"}),
    ("beef_main", {"beef", "veal"}),
]

PROCESSED_BAR_SELECTION_PENALTY = 10.0
AS_INGREDIENT_SELECTION_PENALTY = 8.0

PROCESSED_SNACK_SIDE_TERMS = {
    "pretzel",
    "pretzels",
    "cracker",
    "crackers",
    "crispbread",
    "chips",
    "cheese puffs",
    "snack mix",
}

PROCESSED_SNACK_SIDE_PENALTY = 18.0

# Vitamin D is food-first but does not make an otherwise valid plan infeasible.
VITAMIN_D_SOFT_TARGET_WEIGHT = 35.0

# Breakfast mains should resemble actual breakfast dishes rather than
# sauces, powders or components.
BREAKFAST_MAIN_TERMS = {
    "oatmeal",
    "oats",
    "porridge",
    "cereal",
    "omelet",
    "omelette",
    "egg sandwich",
    "breakfast sandwich",
    "pancake",
    "waffle",
    "french toast",
    "bagel",
    "english muffin",
    "yogurt",
    "yoghurt",
    "smoothie",
}

# Lunch/supper mains should resemble complete dishes.
MAIN_DISH_TERMS = {
    "curry",
    "biryani",
    "sandwich",
    "burger",
    "stew",
    "casserole",
    "lasagna",
    "lasagne",
    "spaghetti",
    "pasta",
    "fried rice",
    "rice mixed",
    "taco",
    "burrito",
    "quesadilla",
    "enchilada",
    "fajita",
    "soup",
    "chili",
    "stir fry",
    "stir-fry",
    "meatloaf",
    "pot pie",
    "dumpling",
    "kebab",
    "kabob",
    "pizza",
    "egg salad sandwich",
    "chicken",
    "beef",
    "fish",
    "lentil",
    "bean",
    "chickpea",
    "chickpeas",
}

# Common vegetables receive a preference boost so that the optimiser
# favours more familiar foods when nutrition performance is similar.
COMMON_FRUIT_TERMS = {
    "banana",
    "apple",
    "orange",
    "mango",
    "guava",
    "blueberry",
    "blueberries",
    "strawberry",
    "strawberries",
    "pineapple",
    "pear",
    "peach",
    "grape",
    "grapes",
    "watermelon",
    "melon",
}

UNUSUAL_STANDALONE_FRUIT_TERMS = {
    "tamarind",
    "passion fruit",
}

COMMON_FRUIT_CANDIDATE_BONUS = 15.0
UNUSUAL_FRUIT_CANDIDATE_PENALTY = 12.0

COMMON_VEGETABLE_TERMS = {
    "spinach",
    "broccoli",
    "carrot",
    "carrots",
    "cabbage",
    "green beans",
    "tomato",
    "tomatoes",
    "peas",
    "mixed vegetables",
    "sweet potato",
    "pumpkin",
    "butternut",
    "cauliflower",
    "zucchini",
    "courgette",
}

# Minimum contribution for actual main-meal items.
MAIN_MIN_PROTEIN_G = {
    "breakfast": 5.0,
    "lunch": 8.0,
    "supper": 8.0,
}
MAIN_MIN_ENERGY_KCAL = {
    "breakfast": 180.0,
    "lunch": 250.0,
    "supper": 250.0,
}

SODIUM_OBJECTIVE_WEIGHT = 0.0050
FAT_OBJECTIVE_WEIGHT = 0.0200
CONVENIENCE_SELECTION_PENALTY = 4.0
LIMIT_SELECTION_PENALTY = 8.0
HIGH_SODIUM_SELECTION_PENALTY = 5.0
ENERGY_DENSE_SELECTION_PENALTY = 1.5

# Curated recipes are preferred as practical lunch/supper mains when a
# nutritionally comparable solution exists. USDA prepared dishes remain
# available as a fallback.
CURATED_RECIPE_CANDIDATE_BONUS = 35.0
CURATED_RECIPE_SELECTION_REWARD = 3.0

CURATED_RECIPE_CUISINE_REWARD = 18.0
DESSERT_SELECTION_PENALTY = 12.0

# Practical supporting-food limits and reporting references.
CEREAL_SIDE_GRAM_BOUNDS = (20.0, 60.0)
NUTS_SEEDS_REPORTING_PORTION_G = 15.0

REPORTING_FALLBACK_DESCRIPTIONS = {
    "fallback_practical_seed_portion": "tablespoon",
    "fallback_practical_reporting_portion": "cup",
    "fallback_practical_berry_cup": "cup",
}

# Runtime semantic safety net for diet labels. The catalogue diet_class is
# still the primary filter, but these terms prevent obvious metadata errors
# such as fish or meat being recommended on vegetarian/vegan plans.
VEGETARIAN_ANIMAL_TERMS = {
    "beef", "veal", "pork", "ham", "bacon", "lamb", "mutton",
    "chicken", "turkey", "duck", "sausage", "pepperoni", "salami",
    "fish", "salmon", "tuna", "cod", "haddock", "tilapia", "sardine",
    "anchovy", "eel", "shrimp", "prawn", "crab", "lobster", "clam",
    "mussel", "oyster", "shellfish"
}

VEGAN_EGG_TERMS = {
    "egg", "eggs", "omelet", "omelette", "mayonnaise"
}

VEGAN_DAIRY_TERMS = {
    "cheese", "yogurt", "yoghurt", "kefir", "buttermilk",
    "butter", "cream", "whey", "casein"
}

PLANT_MILK_PHRASES = {
    "coconut milk", "almond milk", "soy milk", "soya milk",
    "oat milk", "rice milk", "plant milk"
}

# Supporting-food rows must look like foods a mother would reasonably eat
# as a side/snack component, not database artefacts or dessert fillings.
SIDE_QUALITY_EXCLUDE_TERMS = {
    "pie filling", "cake filling", "pastry filling", "sweet potato paste",
    "fruit paste", "paste", "sushi", "candy", "syrup", "frosting",
    "icing", "dessert topping", "pudding", "custard", "sauce", "gravy",
    "dressing", "topping", "baby food", "infant food", "eggnog",
    "candied", "milkshake", "dessert beverage", "sweetened beverage",
    "fruit drink", "dip", "ketchup", "condiment"
}

# USDA prepared dishes do not always expose ingredient-level composition.
# These conservative aliases catch dishes whose names inherently imply an
# excluded staple even when the literal excluded word is absent.
EXCLUDED_FOOD_SEMANTIC_ALIASES = {
    "rice": {"biryani", "biriyani", "biriani", "pilaf", "pilau", "pulao", "risotto", "paella", "jambalaya", "fried rice", "rice bowl", "rice casserole", "rice pudding"},
    "pasta": {"spaghetti", "lasagna", "lasagne", "macaroni", "ravioli", "tortellini", "fettuccine", "linguine", "penne"},
    "noodles": {"noodle", "noodles", "ramen", "udon", "soba", "lo mein", "chow mein"},
    "milk": {"yogurt", "yoghurt", "kefir", "buttermilk", "cream", "cheese", "custard", "ice cream"},
}

# Supporting foods should be simple foods/sides rather than complete mixed
# prepared dishes that happen to be labelled as ingredients in USDA/FNDDS.
SIDE_MIXED_DISH_EXCLUDE_TERMS = {
    "macaroni and cheese", "macaroni or noodles with cheese", "noodles with cheese",
    "pasta with cheese", "pizza", "sandwich", "burger", "burrito", "taco",
    "quesadilla", "enchilada", "lasagna", "lasagne", "spaghetti with",
    "pasta with meat", "pasta with chicken", "pasta with seafood",
    "noodles with meat", "noodles with chicken", "noodles with seafood",
    "fried rice", "biryani", "biriyani", "pilaf", "pilau", "pulao",
    "casserole", "meatloaf", "pot pie", "stir fry", "stir-fry"
}
SIDE_STANDALONE_QUALITY_EXCLUDE_TERMS = {
    "oats, raw","oat, raw","uncooked oats","raw oatmeal",
    "milk, condensed, sweetened","sweetened condensed milk",
    "coconut cream, canned, sweetened","sweetened coconut cream",
    "hot chocolate","cocoa, with whipped cream","with whipped cream",
    "vegetable tempura","potato patty","potato patties",
    "jai, monk's food","monk's food",
    "frozen coffee drink","frozen coffee beverage",
    "coffee drink with whipped cream","coffee beverage with whipped cream",
    "frappuccino","frappe coffee","coffee frappe"
}
PREPARED_COFFEE_BEVERAGE_TERMS = {
    "iced latte", "cafe latte", "café latte", "latte",
    "cappuccino", "mocha", "macchiato", "frappuccino",
    "coffee frappe", "frappe coffee", "frozen coffee",
    "coffee drink", "coffee beverage"
}

def is_prepared_coffee_beverage(row) -> bool:
    """Reject prepared coffee-shop-style beverages as visible supporting foods."""
    text = row_text(row)
    return contains_any_term(text, PREPARED_COFFEE_BEVERAGE_TERMS)

MAX_ORDINARY_FRUIT_SIDE_G = 200.0
FIBRE_SOFT_THRESHOLD_G = 50.0
FIBRE_EXCESS_PENALTY_WEIGHT = 20.0

# VERIFIED LACTATION MICRONUTRIENT TARGETS
# Vitamin B1 is energy-dependent and calcium is age-dependent.

BASE_MICRONUTRIENT_TARGETS = {
    "vitamin-a_100g": {
        "label": "Vitamin A",
        "unit": "µg",
        "value": 1100.0,
        "reference_type": "AI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "vitamin-b2_100g": {
        "label": "Vitamin B2",
        "unit": "mg",
        "value": 2.0,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "vitamin-b6_100g": {
        "label": "Vitamin B6",
        "unit": "mg",
        "value": 1.7,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "folates_100g": {
        "label": "Folate",
        "unit": "µg",
        "value": 500.0,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "vitamin-b12_100g": {
        "label": "Vitamin B12",
        "unit": "µg",
        "value": 3.8,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "vitamin-c_100g": {
        "label": "Vitamin C",
        "unit": "mg",
        "value": 135.0,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "vitamin-d_100g": {
        "label": "Vitamin D",
        "unit": "µg",
        "value": 10.0,
        "reference_type": "AI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "iron_100g": {
        "label": "Iron",
        "unit": "mg",
        "value": 16.0,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "magnesium_100g": {
        "label": "Magnesium",
        "unit": "mg",
        "value": 300.0,
        "reference_type": "AI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "zinc_100g": {
        "label": "Zinc",
        "unit": "mg",
        "value": 9.9,
        "reference_type": "PRI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
    "selenium_100g": {
        "label": "Selenium",
        "unit": "µg",
        "value": 85.0,
        "reference_type": "AI",
        "age_condition": "Adult lactating women",
        "source": "Health Council of the Netherlands (2024)",
    },
}

# REPORTING NUTRIENTS

INFORMATION_NUTRIENTS = {
    "fat_100g": ("Fat", "g"),
    "carbohydrates_100g": ("Carbohydrates", "g"),
    "fiber_100g": ("Fibre", "g"),
    "sodium_100g": ("Sodium", "mg"),
}

# COUNTRIES / GAM INPUT

COUNTRIES = {
    "Nigeria",
    "Kenya",
    "Ethiopia",
    "Burundi",
    "South Africa",
}

EXPECTED_GAM_COLUMNS = [
    "AGE",
    "NUMBER_LIVING_CHILDREN",
    "WEIGHT",
    "HEIGHT",
    "CURRENTLY_PREGNANT",
    "CHILD_AGE_MONTHS",
    "LACTATION_STAGE_Early lactation",
    "LACTATION_STAGE_Mid lactation",
    "LACTATION_STAGE_Late lactation",
    "COUNTRY_Nigeria",
    "COUNTRY_Kenya",
    "COUNTRY_Ethiopia",
    "COUNTRY_Burundi",
    "COUNTRY_South Africa",
]

# ALLERGY FIELD MAPPING

ALLERGY_FIELDS = {
    "milk": ["contains_milk"],
    "dairy": ["contains_milk"],
    "lactose": ["contains_milk"],
    "egg": ["contains_egg"],
    "eggs": ["contains_egg"],
    "peanut": ["contains_peanuts"],
    "peanuts": ["contains_peanuts"],
    "tree nuts": ["contains_tree_nuts"],
    "tree nut": ["contains_tree_nuts"],
    "nuts": [
        "contains_peanuts",
        "contains_tree_nuts",
    ],
    "soy": ["contains_soy"],
    "soya": ["contains_soy"],
    "fish": ["contains_fish"],
    "shellfish": ["contains_shellfish"],
    "gluten": ["contains_gluten"],
}

# CUISINE NORMALISATION
# UI labels and historical USDA/FNDDS cuisine labels are mapped to a common
# canonical value so, for example, the UI option "Asian" can also recognise
# existing "East/SE Asian" records.
CUISINE_ALIASES = {
    "indian": "indian",
    "asian": "asian",
    "east/se asian": "asian",
    "east southeast asian": "asian",
    "east/southeast asian": "asian",
    "east and southeast asian": "asian",
    "mediterranean": "mediterranean",
    "middle eastern/mediterranean": "middle eastern",
    "middle eastern": "middle eastern",
    "latin american": "latin american",
    "mexican": "latin american",
    "mexican/tex mex": "latin american",
    "mexican/tex-mex": "latin american",
    "north american": "north american",
    "european": "european",
    "italian": "european",
    "african": "african",
    "general": "general",
}

# LOAD MODELS AND FOOD DATA

for required_file in [FOOD_FILE, RECIPE_FILE, PROTEIN_MODEL_FILE, ENERGY_MODEL_FILE, ]:
    if not os.path.exists(required_file):
        raise RuntimeError(f"Required file is missing: {required_file}")

protein_model = joblib.load(PROTEIN_MODEL_FILE)
energy_model = joblib.load(ENERGY_MODEL_FILE)
usda_foods = pd.read_csv(FOOD_FILE, low_memory=False, )
recipe_foods = pd.read_csv(RECIPE_FILE, low_memory=False, )

USDA_SOURCE_ROW_COUNT = int(len(usda_foods))
RECIPE_SOURCE_ROW_COUNT = int(len(recipe_foods))

all_food_columns = sorted(
    set(usda_foods.columns)
    | set(recipe_foods.columns)
)

usda_foods = usda_foods.reindex(
    columns=all_food_columns,
)

recipe_foods = recipe_foods.reindex(
    columns=all_food_columns,
)

foods = pd.concat(
    [usda_foods, recipe_foods],
    ignore_index=True,
    sort=False,
)

# VALIDATE USDA DATASET

REQUIRED_FOOD_COLUMNS = {
    "catalogue_id",
    "fdc_id",
    "display_name",
    "description",
    "candidate_type",
    "food_group",
    "meal_type",
    "cuisine",
    "diet_class",
    "lp_use_class",
    "energy_100g",
    "proteins_100g",
}

missing_food_columns = sorted(REQUIRED_FOOD_COLUMNS - set(foods.columns))

if missing_food_columns:
    raise RuntimeError("usda_lp_food_catalogue.csv is missing required columns: " + ", ".join(missing_food_columns))

REQUIRED_RECIPE_COLUMNS = {
    "recipe_id",
    "catalogue_id",
    "display_name",
    "candidate_type",
    "meal_type",
    "cuisine",
    "diet_class",
    "ingredient_names",
    "ingredient_keys",
    "energy_100g",
    "proteins_100g",
    "serving_size_g",
    "contains_milk",
    "contains_egg",
    "contains_peanuts",
    "contains_tree_nuts",
    "contains_soy",
    "contains_fish",
    "contains_shellfish",
    "contains_gluten",
}

missing_recipe_columns = sorted(REQUIRED_RECIPE_COLUMNS - set(recipe_foods.columns))

if missing_recipe_columns:
    raise RuntimeError("recipe_catalogue_final.csv is missing required columns: " + ", ".join(missing_recipe_columns))

# Make important numerical columns numeric once at startup.
NUMERIC_COLUMNS = [
    "energy_100g",
    "proteins_100g",
    "fat_100g",
    "carbohydrates_100g",
    "fiber_100g",
    "calcium_100g",
    "iron_100g",
    "vitamin-a_100g",
    "vitamin-b12_100g",
    "vitamin-c_100g",
    "vitamin-d_100g",
    "vitamin-b1_100g",
    "vitamin-b2_100g",
    "vitamin-b6_100g",
    "folates_100g",
    "magnesium_100g",
    "zinc_100g",
    "sodium_100g",
    "selenium_100g",
    "serving_size_g",
    "nutrient_coverage_pct",
]

for column in NUMERIC_COLUMNS:
    if column in foods.columns:
        foods[column] = pd.to_numeric(
            foods[column],
            errors="coerce",
        )

foods = foods[
    foods["energy_100g"].notna()
    & (foods["energy_100g"] > 0)
    & foods["proteins_100g"].notna()
    & (foods["proteins_100g"] >= 0)
].copy()

foods.reset_index(drop=True, inplace=True, )

# GENERAL HELPERS

def normalize_text(value) -> str:
    if value is None:
        return ""

    try:
        if pd.isna(value):
            return ""
    except Exception:
        pass

    text = str(value).strip().lower()

    text = (
        text
        .replace("_", " ")
        .replace("-", " ")
    )

    text = re.sub(
        r"\s+",
        " ",
        text,
    )
    return text

def as_list(value) -> List[str]:
    if value is None:
        return []

    if isinstance(value, list):
        return [
            str(item).strip()
            for item in value
            if str(item).strip()
        ]
    text = str(value).strip()

    if (
        not text
        or text.lower() == "none"
    ):
        return []
    return [text]

def normalized_list(values) -> List[str]:
    result = []
    seen = set()

    for value in values:
        normal = normalize_text(value)
        if (
            normal
            and normal not in seen
        ):
            seen.add(normal)
            result.append(normal)
    return result

def parse_positive_float(
    value,
    field_name: str,
) -> float:
    try:
        number = float(value)
    except (TypeError, ValueError) as exc:
        raise ValueError(f"{field_name} must be a number.") from exc

    if (
        not math.isfinite(number)
        or number <= 0
    ):
        raise ValueError(f"{field_name} must be greater than 0.")
    return number

def parse_nonnegative_int(
    value,
    field_name: str,
) -> int:
    try:
        number = int(value)
    except (TypeError, ValueError) as exc:
        raise ValueError(f"{field_name} must be a whole number.") from exc

    if number < 0:
        raise ValueError(f"{field_name} cannot be negative.")
    return number

def to_bool(value) -> bool:
    if isinstance(value, bool):
        return value

    if value is None:
        return False

    try:
        if pd.isna(value):
            return False
    except Exception:
        pass

    return (
        str(value).strip().lower()
        in {
            "true",
            "1",
            "yes",
            "y",
        }
    )

def contains_term(
    text: str,
    term: str,
) -> bool:
    text = normalize_text(text)
    term = normalize_text(term)

    if (
        not text
        or not term
    ):
        return False

    pattern = (
        r"(?<![a-z0-9])"
        + re.escape(term)
        + r"(?![a-z0-9])"
    )

    return bool(
        re.search(
            pattern,
            text,
            flags=re.I,
        )
    )

def contains_any_term(
    text: str,
    terms,
) -> bool:
    return any(
        contains_term(
            text,
            term,
        )
        for term in terms
    )

def safe_float(
    value,
    default: float = 0.0,
) -> float:
    try:
        number = float(value)
    except (TypeError, ValueError):
        return default

    if not math.isfinite(number):
        return default

    return number

def canonical_cuisine(
    value,
) -> str:
    normal = normalize_text(value)

    if not normal:
        return ""

    return CUISINE_ALIASES.get(
        normal,
        normal,
    )

def is_curated_recipe(
    row,
) -> bool:
    return (
        normalize_text(
            row.get(
                "source",
                "",
            )
        )
        == "curated usda recipe"
        or normalize_text(
            row.get(
                "curation_status",
                "",
            )
        )
        == "curated recipe"
        or str(
            row.get(
                "catalogue_id",
                "",
            )
        ).upper().startswith(
            "REC-"
        )
    )

def exclusion_search_text(
    row,
) -> str:
    """
    Searchable text used for excluded-food filtering and the final safety
    re-check. Curated recipes carry ingredient_names and ingredient_keys,
    allowing an exclusion such as 'chicken' to remove a recipe even when the
    recipe name itself does not contain that exact word.
    """
    fields = [
        "display_name",
        "description",
        "ingredient_names",
        "ingredient_keys",
    ]

    return " ".join(
        normalize_text(
            row.get(
                field,
                "",
            )
        )
        for field in fields
    )

GLUTEN_SEMANTIC_RISK_TERMS = {
    "bagel","bread","bun","roll","croissant","muffin","cake","cupcake",
    "cookie","biscuit","cracker","pretzel","pancake","waffle","wheat",
    "barley","rye","couscous","bulgur","seitan","spaghetti","macaroni",
    "lasagna","lasagne","fettuccine","linguine","penne","ravioli",
    "tortellini","noodle","noodles","lefse","pita","naan"
}

def has_gluten_semantic_conflict(row) -> bool:
    """Catch obvious gluten foods incorrectly labelled contains_gluten=no."""
    text = " ".join([
        normalize_text(row.get("display_name", "")),
        normalize_text(row.get("description", "")),
        normalize_text(row.get("ingredient_names", "")),
    ])
    if any(marker in text for marker in ("gluten free","without gluten","no gluten")):
        return False
    return contains_any_term(text, GLUTEN_SEMANTIC_RISK_TERMS)

PLANT_MILK_MARKERS = {
    "soy milk", "soya milk", "almond milk", "oat milk", "coconut milk",
    "rice milk", "cashew milk", "hemp milk", "pea milk", "flax milk",
    "plant milk", "plant-based milk", "non-dairy milk", "nondairy milk"
}

def excluded_food_matches(row, term: str) -> bool:
    """Literal match plus conservative semantic aliases for user exclusions."""
    term = normalize_text(term)
    searchable = exclusion_search_text(row)

    if term == "milk":
        name_text = " ".join([
            normalize_text(row.get("display_name", "")),
            normalize_text(row.get("description", "")),
        ])
        clearly_plant_milk = contains_any_term(name_text, PLANT_MILK_MARKERS)

        # Preserve clearly named plant alternatives such as oat/coconut/soy milk.
        if contains_term(searchable, term) and not clearly_plant_milk:
            return True
        if clearly_plant_milk:
            return False

        # Treat obvious dairy derivatives as part of a user-facing milk exclusion.
        return contains_any_term(name_text, EXCLUDED_FOOD_SEMANTIC_ALIASES["milk"])

    if contains_term(searchable, term):
        return True

    aliases = EXCLUDED_FOOD_SEMANTIC_ALIASES.get(term, set())
    if not aliases:
        return False

    name_text = " ".join([
        normalize_text(row.get("display_name", "")),
        normalize_text(row.get("description", "")),
    ])
    return contains_any_term(name_text, aliases)

def has_vegetarian_animal_conflict(row) -> bool:
    text = row_text(row)
    group = normalize_text(row.get("food_group", ""))

    if group in {
        "meat & poultry",
        "fish & seafood",
    }:
        return True

    return contains_any_term(text, VEGETARIAN_ANIMAL_TERMS)

def has_vegan_animal_conflict(row) -> bool:
    if has_vegetarian_animal_conflict(row):
        return True

    text = row_text(row)
    group = normalize_text(row.get("food_group", ""))

    if group in {"eggs", "dairy"}:
        return True

    if contains_any_term(text, VEGAN_EGG_TERMS):
        return True

    if contains_any_term(text, VEGAN_DAIRY_TERMS):
        return True

    # Generic "milk" is treated as dairy unless the text clearly refers
    # to one of the plant-milk phrases above.
    if contains_term(text, "milk"):
        if not contains_any_term(text, PLANT_MILK_PHRASES):
            return True
    return False

def is_practical_supporting_food(row) -> bool:
    """Reject obvious database artefacts and prepared/dessert-like sides."""
    text = row_text(row)

    if is_prepared_coffee_beverage(row):
        return False

    if contains_any_term(text, SIDE_QUALITY_EXCLUDE_TERMS | SIDE_MIXED_DISH_EXCLUDE_TERMS | SIDE_STANDALONE_QUALITY_EXCLUDE_TERMS):
        return False

    if is_supplement_like_food(row) or is_concentrated_ingredient_food(row):
        return False

    return True

# GAM HELPERS

def get_model_columns(
    model,
) -> List[str]:
    columns = getattr(
        model,
        "feature_names_in_",
        None,
    )

    if columns is not None:
        return list(columns)

    return EXPECTED_GAM_COLUMNS.copy()

def prepare_gam_input(
    age: int,
    children: int,
    weight: float,
    height: float,
    pregnant: str,
    child_age: int,
    lactation_stage: str,
    country: str,
    model,
) -> pd.DataFrame:

    base = pd.DataFrame([{
        "AGE": age,
        "NUMBER_LIVING_CHILDREN": children,
        "WEIGHT": weight,
        "HEIGHT": height,
        "CURRENTLY_PREGNANT": (
            1
            if pregnant == "yes"
            else 0
        ),
        "CHILD_AGE_MONTHS": child_age,
        "LACTATION_STAGE": lactation_stage,
        "COUNTRY": country,
    }])

    encoded = pd.get_dummies(
        base,
        columns=[
            "LACTATION_STAGE",
            "COUNTRY",
        ],
    )

    expected = get_model_columns(model)

    for column in expected:
        if column not in encoded.columns:
            encoded[column] = 0

    return encoded.reindex(
        columns=expected,
        fill_value=0,
    )

# MICRONUTRIENT TARGETS

def build_micronutrient_targets(
    age: int,
    energy_req_kcal: float,
) -> Dict[str, dict]:

    targets = {
        key: dict(value)
        for key, value
        in BASE_MICRONUTRIENT_TARGETS.items()
    }

    energy_mj = (energy_req_kcal * 4.184 / 1000.0)

    targets["vitamin-b1_100g"] = {
        "label": "Vitamin B1",
        "unit": "mg",
        "value": (0.1 * energy_mj + 0.16),
        "reference_type": "PRI",
        "age_condition": "Energy-dependent lactation value",
        "source": "Health Council of the Netherlands (2024)",
    }

    calcium_value = (
        1000.0
        if 18 <= age <= 24
        else 950.0
    )

    targets["calcium_100g"] = {
        "label": "Calcium",
        "unit": "mg",
        "value": calcium_value,
        "reference_type": "PRI",
        "age_condition": (
            "Age 18–24"
            if age <= 24
            else "Age 25+"
        ),
        "source": "Health Council of the Netherlands (2024)",
    }

    return targets

def determine_active_targets(candidates: pd.DataFrame, targets: Dict[str, dict], diets: Optional[List[str]] = None) -> Tuple[Dict[str, dict], List[str]]:
    active, notes = {}, []
    normalized_diets = set(normalized_list(diets or []))
    report_only = {"vitamin-d_100g"} | ({"vitamin-b12_100g"} if "vegan" in normalized_diets else set())

    for column, config in targets.items():
        if column in report_only:
            if column == "vitamin-d_100g":
                notes.append(f"{config['label']} remains a displayed 10 µg/day planning target and is optimised food-first, but it is not a hard feasibility constraint because reliable dietary Vitamin D sources are limited in some eligible food selections.")
            else:
                notes.append(f"{config['label']} remains a displayed planning target but is not used as a hard optimisation constraint for vegan plans because the current eligible plant-based catalogue does not provide a sufficiently reliable dietary source.")
            continue
        if column not in candidates.columns:
            notes.append(f"{config['label']} could not be constrained because that nutrient is not present in the current USDA LP catalogue.")
            continue
        values = pd.to_numeric(candidates[column], errors="coerce")
        if int((values.notna() & (values > 0)).sum()) < 3:
            notes.append(f"{config['label']} was not used as a hard constraint because too few eligible foods contained a usable value.")
            continue
        active[column] = config
    return active, notes

# FOOD FILTERING

def apply_filters(
    source: pd.DataFrame,
    diets: List[str],
    allergies: List[str],
    cuisines: List[str],
    excluded_foods: List[str],
) -> Tuple[
    pd.DataFrame,
    List[str],
]:

    df = source.copy()

    warnings: List[str] = []

    # Remove supplement-like/powder products from recommendation use.
    supplement_mask = df.apply(is_supplement_like_food, axis=1,)

    removed_supplements = int(
        supplement_mask.sum()
    )

    if removed_supplements:
        df = df[
            ~supplement_mask
        ].copy()

        warnings.append(
            f"{removed_supplements:,} supplement-like or nutritional "
            "powder products were excluded from recommendation use."
        )

    concentrated_mask = df.apply(is_concentrated_ingredient_food, axis=1,)

    removed_concentrated = int(concentrated_mask.sum())

    if removed_concentrated:
        df = df[
            ~concentrated_mask
        ].copy()

        warnings.append(
            f"{removed_concentrated:,} concentrated ingredient powder "
            "foods such as dry milk powder were excluded from standalone "
            "recommendation use."
        )

    diets = normalized_list(diets)
    allergies = normalized_list(allergies)
    cuisines = normalized_list(cuisines)
    excluded_foods = normalized_list(excluded_foods)

    # EXCLUDED FOODS

    if excluded_foods:
        exclusion_mask = pd.Series(False, index=df.index,)

        for term in excluded_foods:
            current = df.apply(lambda row: excluded_food_matches(row, term), axis=1)
            exclusion_mask |= current

        removed_excluded = int(exclusion_mask.sum())

        df = df[~exclusion_mask].copy()

        if removed_excluded:
            warnings.append(
                f"Excluded-food filtering removed {removed_excluded:,} "
                "foods or recipes. Curated recipes were checked using "
                "their ingredient names and ingredient keys as well as "
                "their recipe names. USDA prepared dishes also use conservative "
                "semantic aliases for exclusions such as rice in biryani."
            )

    # ALLERGIES
    # Only explicit "no" passes for a selected allergen.
    # "unknown" is deliberately removed.

    for allergy in allergies:
        fields = ALLERGY_FIELDS.get(allergy,)

        if not fields:
            warnings.append(
                f"The selected allergy '{allergy}' does not have "
                "a dedicated metadata field in the current catalogue."
            )
            continue

        before = len(df)

        safe_mask = pd.Series(True, index=df.index,)

        for field in fields:
            if field not in df.columns:
                safe_mask &= False
                continue

            values = (
                df[field]
                .fillna("unknown")
                .astype(str)
                .str.strip()
                .str.lower()
            )

            safe_mask &= (values == "no")

        df = df[safe_mask].copy()
        removed = (before - len(df))

        warnings.append(
            f"Allergy filtering for '{allergy}' was conservative: "
            f"{removed:,} foods were removed because they were "
            "labelled yes or unknown for that allergen."
        )

    # DIET

    if "vegan" in diets:
        df = df[
            df["diet_class"]
            .fillna("")
            .astype(str)
            .str.lower()
            .eq("vegan_candidate")
        ].copy()

        semantic_conflict = df.apply(has_vegan_animal_conflict, axis=1,)
        removed_conflicts = int(semantic_conflict.sum())
        if removed_conflicts:
            df = df[~semantic_conflict].copy()
            warnings.append(
                f"Diet-safety re-check removed {removed_conflicts:,} foods "
                "whose names or ingredients conflicted with a vegan diet."
            )

    elif "vegetarian" in diets:
        allowed = {
            "vegetarian",
            "vegan_candidate",
        }

        df = df[
            df["diet_class"]
            .fillna("")
            .astype(str)
            .str.lower()
            .isin(allowed)
        ].copy()

        semantic_conflict = df.apply(has_vegetarian_animal_conflict, axis=1,)
        removed_conflicts = int(semantic_conflict.sum())
        if removed_conflicts:
            df = df[~semantic_conflict].copy()
            warnings.append(
                f"Diet-safety re-check removed {removed_conflicts:,} foods "
                "whose names or ingredients conflicted with a vegetarian diet."
            )

    if (
        "gluten free" in diets
        or "gluten-free" in diets
    ):
        if "contains_gluten" in df.columns:
            df = df[
                df["contains_gluten"]
                .fillna("unknown")
                .astype(str)
                .str.lower()
                .eq("no")
            ].copy()

            semantic_gluten_conflict = df.apply(has_gluten_semantic_conflict, axis=1)
            removed_semantic_gluten = int(semantic_gluten_conflict.sum())
            if removed_semantic_gluten:
                df = df[~semantic_gluten_conflict].copy()

            warnings.append(
                "Gluten-free filtering used only foods explicitly labelled as not containing gluten. "
                f"A semantic safety backstop also removed {removed_semantic_gluten:,} records whose names or ingredients "
                "still implied a gluten-containing food unless they were explicitly described as gluten-free."
            )

    if (
        "lactose free" in diets
        or "lactose-free" in diets
    ):
        if "contains_milk" in df.columns:
            df = df[
                df["contains_milk"]
                .fillna("unknown")
                .astype(str)
                .str.lower()
                .eq("no")
            ].copy()

            warnings.append(
                "The lactose-free option currently uses the "
                "conservative milk-allergen field because a "
                "dedicated lactose field is not available."
            )

    # CUISINE
    # Cuisine applies to prepared dishes. Ingredients remain available
    # as supporting foods because fruit, vegetables, milk, grains, etc.
    # are not meaningful cuisine-specific records.

    if cuisines:
        requested = {
            canonical_cuisine(value)
            for value in cuisines
            if canonical_cuisine(value)
        }

        prepared_mask = (
            df["candidate_type"]
            .astype(str)
            .str.lower()
            .eq("prepared_dish")
        )

        cuisine_values = (
            df["cuisine"]
            .fillna("")
            .map(canonical_cuisine)
        )

        matching_prepared = (
            prepared_mask
            & cuisine_values.isin(
                requested
            )
        )

        ingredient_mask = (~prepared_mask)

        if int(
            matching_prepared.sum()
        ) == 0:
            warnings.append(
                "No prepared dishes matched the selected cuisine "
                "after the other safety filters were applied."
            )

        df = df[
            ingredient_mask
            | matching_prepared
        ].copy()

    return (
        df.reset_index(
            drop=True,
        ),
        warnings,
    )

# SERVING / CANDIDATE HELPERS

def serving_grams(
    row,
    role: str,
) -> Tuple[
    float,
    str,
]:

    source_value = safe_float(
        row.get(
            "serving_size_g",
        ),
        default=0.0,
    )

    if (
        5.0
        <= source_value
        <= 600.0
    ):
        if is_curated_recipe(
            row
        ):
            return (
                source_value,
                "curated_recipe_serving",
            )

        return (
            source_value,
            "USDA household portion",
        )

    group = str(
        row.get(
            "food_group",
            "",
        )
    ).strip()

    if (
        role == "side"
        and group
        in GROUP_FALLBACK_SERVINGS_G
    ):
        return (
            GROUP_FALLBACK_SERVINGS_G[group],
            "fallback_group_portion",
        )

    return (
        FALLBACK_SERVINGS_G.get(
            role,
            100.0,
        ),
        "fallback_role_portion",
    )


def reporting_serving_grams(
    row,
    role: str,
) -> Tuple[
    float,
    str,
]:
    """
    Return a serving reference for DISPLAY only.

    Nutrient calculations still use the selected grams and USDA
    per-100-g nutrient values directly.

    Some USDA household portions are technically valid but misleading
    for reporting. For example, kale may use "1 leaf" (~10 g), which
    can make a normal vegetable portion appear as 10+ servings.
    In those cases a practical reporting comparison is used instead.
    """

    serving, source = serving_grams(
        row,
        role,
    )

    group = str(
        row.get(
            "food_group",
            "",
        )
    ).strip()

    description = normalize_text(
        row.get(
            "serving_description",
            "",
        )
    )

    if (
        role == "side"
        and group == "Nuts & Seeds"
        and serving > 60.0
    ):
        return (
            NUTS_SEEDS_REPORTING_PORTION_G,
            "fallback_practical_seed_portion",
        )

    berry_text = row_text(row)

    if (
        role == "side"
        and group == "Fruit"
        and contains_any_term(
            berry_text,
            {
                "blackberry",
                "blackberries",
                "blueberry",
                "blueberries",
                "strawberry",
                "strawberries",
                "raspberry",
                "raspberries",
            },
        )
        and (
            serving < 70.0
            or contains_any_term(
                description,
                {
                    "berry",
                    "blackberry",
                    "blueberry",
                    "strawberry",
                    "raspberry",
                },
            )
        )
    ):
        return (140.0, "fallback_practical_berry_cup",)

    if (
        role == "side"
        and group == "Vegetables"
        and source == "USDA household portion"
        and (
            serving < MIN_USDA_VEGETABLE_REPORTING_PORTION_G
            or contains_term(
                description,
                "leaf",
            )
        )
    ):
        return (
            REPORTING_FALLBACK_SERVINGS_G[
                "Vegetables"
            ],
            "fallback_practical_reporting_portion",
        )

    return (serving, source,)

def role_gram_bounds(
    row,
    role: str,
) -> Tuple[
    float,
    float,
    float,
    str,
]:

    serving, source = serving_grams(row, role,)
    lower_servings, upper_servings = (MEAL_SERVING_RANGES[role])
    lower = (serving * lower_servings)
    upper = (serving * upper_servings)

    if role == "side":
        group = str(
            row.get(
                "food_group",
                "",
            )
        ).strip()

        upper = min(
            upper,
            SIDE_GROUP_MAX_GRAMS.get(
                group,
                250.0,
            ),
        )

        subtype = side_food_subtype(row)
        subtype_bounds = (SIDE_SUBTYPE_GRAM_BOUNDS.get(subtype))

        if subtype_bounds:
            subtype_lower, subtype_upper = (subtype_bounds)
            lower = max(lower, subtype_lower,)
            upper = min(upper, subtype_upper,)

            # If a USDA serving is unusually large, the practical subtype
            # bound takes priority so the solver is not forced into an
            # unrealistic portion.
            if upper < lower:
                lower = subtype_lower
                upper = subtype_upper

        if subtype == "fresh_fruit":
            upper = min(upper, MAX_ORDINARY_FRUIT_SIDE_G)
            if upper < lower: lower = min(lower, upper)

        if is_cereal_supporting_food(row):
            cereal_lower, cereal_upper = CEREAL_SIDE_GRAM_BOUNDS
            lower = max(min(lower, cereal_upper), cereal_lower)
            upper = min(upper, cereal_upper)
            if upper < lower:
                lower = cereal_lower
                upper = cereal_upper

    return (
        max(
            5.0,
            lower,
        ),
        max(
            lower,
            upper,
        ),
        serving,
        source,
    )

def row_text(
    row,
) -> str:
    return " ".join([
        normalize_text(
            row.get(
                "display_name",
                "",
            )
        ),
        normalize_text(
            row.get(
                "description",
                "",
            )
        ),
        normalize_text(
            row.get(
                "source_food_category",
                "",
            )
        ),
        normalize_text(
            row.get(
                "ingredient_names",
                "",
            )
        ),
        normalize_text(
            row.get(
                "ingredient_keys",
                "",
            )
        ),
    ])

def is_supplement_like_food(
    row,
) -> bool:
    return contains_any_term(row_text(row), SUPPLEMENT_LIKE_TERMS,)

def is_concentrated_ingredient_food(
    row,
) -> bool:
    return contains_any_term(row_text(row), CONCENTRATED_INGREDIENT_TERMS,)

def side_food_subtype(
    row,
) -> str:
    """
    Classify supporting foods into practical portion subtypes.
    """

    group = normalize_text(
        row.get(
            "food_group",
            "",
        )
    )

    text = row_text(row)

    if group == "fruit":
        if contains_any_term(
            text,
            DRIED_FRUIT_TERMS,
        ):
            return "dried_fruit"

        return "fresh_fruit"

    if group == "nuts & seeds":
        return "nuts_seeds"

    if group == "dairy":
        return "ordinary_dairy"

    if group == "vegetables":
        if contains_any_term(
            text,
            {
                "mushroom",
                "mushrooms",
            },
        ) and contains_any_term(
            text,
            COOKED_VEGETABLE_TERMS,
        ):
            return "cooked_mushroom"

        if (
            contains_any_term(
                text,
                RAW_LEAFY_VEGETABLE_TERMS,
            )
            and "raw" in text
        ):
            return "raw_leafy_vegetable"

        if contains_any_term(
            text,
            COOKED_VEGETABLE_TERMS,
        ):
            return "cooked_vegetable"

        return "other_vegetable"

    if group == "grains & staples":
        return "grain_staple"

    if group == "legumes & pulses":
        return "legume"

    if group == "eggs":
        return "egg"

    return ""

def is_processed_bar(
    row,
) -> bool:
    return contains_any_term(row_text(row), PROCESSED_BAR_TERMS,)

def is_processed_snack_side(
    row,
) -> bool:
    return contains_any_term(row_text(row), PROCESSED_SNACK_SIDE_TERMS,)

def is_as_ingredient_record(
    row,
) -> bool:
    return contains_any_term(row_text(row), AS_INGREDIENT_TERMS,)

def main_dish_family(
    row,
) -> str:
    text = row_text(row)

    for family, terms in MAIN_DISH_FAMILY_RULES:
        if contains_any_term(
            text,
            terms,
        ):
            return family

    return "other"

def is_ordinary_dairy_candidate(
    row,
) -> bool:
    if normalize_text(
        row.get(
            "food_group",
            "",
        )
    ) != "dairy":
        return False

    if (
        is_concentrated_ingredient_food(
            row
        )
        or is_supplement_like_food(
            row
        )
    ):
        return False

    return contains_any_term(row_text(row), COMMON_DAIRY_TERMS,)

def is_component_only_main(
    row,
) -> bool:
    text = row_text(row)

    if contains_any_term(text, MAIN_COMPONENT_ONLY_TERMS,):
        return True

    # Explicit standalone shell records should not become lunch/supper mains.
    if (contains_term(text, "taco shell") or contains_term(text, "taco shells")):
        return True

    return False

def is_valid_breakfast_main(
    row,
) -> bool:
    if is_component_only_main(row):
        return False

    if is_supplement_like_food(row):
        return False

    text = row_text(row)

    # Require a recognisable breakfast food/dish term.
    return contains_any_term(text, BREAKFAST_MAIN_TERMS,)

def is_valid_main_dish(
    row,
) -> bool:
    if is_component_only_main(row):
        return False

    if is_supplement_like_food(row):
        return False

    text = row_text(row)

    if (is_curated_recipe(row) and normalize_text(row.get("candidate_type", "",)) == "prepared dish"):
        return True

    # Keep prepared dishes that look like complete dishes.
    if contains_any_term(text, MAIN_DISH_TERMS,):
        return True

    # Fallback for USDA prepared dishes with substantial protein density.
    protein_100g = safe_float(row.get("proteins_100g",), 0.0,)

    energy_100g = safe_float(row.get("energy_100g",), 0.0,)

    return (protein_100g >= 6.0 and energy_100g >= 100.0)

def is_common_vegetable(
    row,
) -> bool:
    if not is_whole_vegetable_candidate(row):
        return False

    return contains_any_term(row_text(row), COMMON_VEGETABLE_TERMS,)

def is_common_fruit(
    row,
) -> bool:
    if normalize_text(row.get("food_group", "",)) != "fruit":
        return False

    return contains_any_term(row_text(row), COMMON_FRUIT_TERMS,)

def is_unusual_standalone_fruit(
    row,
) -> bool:
    if normalize_text(row.get("food_group", "",)) != "fruit":
        return False

    return contains_any_term(row_text(row), UNUSUAL_STANDALONE_FRUIT_TERMS,)

def is_breakfast_or_snack_side(
    row,
) -> bool:
    group = normalize_text(row.get("food_group", "",))

    text = row_text(row)

    if group in {"fruit", "dairy", "nuts & seeds",}:
        return True

    if ("cereal" in text or "granola" in text or "oatmeal" in text):
        return True

    return False


def is_lunch_or_supper_side(
    row,
) -> bool:
    group = normalize_text(row.get("food_group", "",))

    text = row_text(row)

    if group in {"vegetables", "legumes & pulses", "grains & staples", "eggs",}:
        # Breakfast cereals should not be attached to lunch/supper.
        if ("cereal" in text or "bran flakes" in text or "oatmeal" in text):
            return False

        return True

    return False

def is_whole_vegetable_candidate(
    row,
) -> bool:
    """
    Only whole/minimally processed vegetable-side records may satisfy
    the mandatory vegetable requirement.
    """

    group = normalize_text(row.get("food_group", "",))

    if group != "vegetables":
        return False

    text = " ".join([normalize_text(row.get("display_name", "",)), normalize_text(row.get("description", "",)),])

    return not contains_any_term(text, PROCESSED_VEGETABLE_TERMS,)


def is_convenience_food(
    row,
) -> bool:
    text = " ".join([normalize_text(row.get("display_name", "",)), normalize_text(row.get("description", "",)),])

    return contains_any_term(text, CONVENIENCE_TERMS,)


def food_quality_penalty(
    row,
) -> float:
    penalty = 0.0

    if is_convenience_food(row):
        penalty += CONVENIENCE_SELECTION_PENALTY

    return penalty


def is_cereal_supporting_food(
    row,
) -> bool:
    return contains_any_term(row_text(row), {"cereal", "bran flakes", "corn flakes", "granola", "muesli"},)


def candidate_score(
    row,
    preferred_cuisines: List[str],
) -> float:

    score = safe_float(row.get("nutrient_coverage_pct",), default=0.0,)

    if (str(row.get("lp_use_class", "",)).upper() == "CORE"):
        score += 20.0
    else:
        score -= 25.0

    if to_bool(row.get("has_household_portion", False,)):
        score += 10.0

    if to_bool(row.get("high_sodium_flag", False,)):
        score -= 10.0

    if to_bool(row.get("very_energy_dense_flag", False,)):
        score -= 4.0

    if preferred_cuisines:
        cuisine = canonical_cuisine(row.get("cuisine", "",))

        if cuisine in {canonical_cuisine(c) for c in preferred_cuisines}:
            score += 25.0

    if is_curated_recipe(row):
        score += CURATED_RECIPE_CANDIDATE_BONUS

    # Small rewards for foods that provide useful protein and fibre.
    score += min(safe_float(row.get("fiber_100g",), 0.0,), 15.0,)

    score += min(safe_float(row.get("proteins_100g",), 0.0,) * 0.5, 12.0,)

    if is_whole_vegetable_candidate(row):
        score += 20.0

    if is_common_vegetable(row):
        score += 18.0

    if is_common_fruit(row):
        score += COMMON_FRUIT_CANDIDATE_BONUS

    if is_unusual_standalone_fruit(row):
        score -= UNUSUAL_FRUIT_CANDIDATE_PENALTY

    if is_ordinary_dairy_candidate(row):
        score += 12.0

    if is_processed_bar(row):
        score -= 22.0

    if is_processed_snack_side(row):
        score -= PROCESSED_SNACK_SIDE_PENALTY

    if is_as_ingredient_record(row):
        score -= 18.0

    if is_convenience_food(row):
        score -= 20.0

    if is_curated_recipe(row):
        score += CURATED_RECIPE_SELECTION_REWARD
        if preferred_cuisines:
            cuisine = normalize_text(row.get("cuisine", ""))
            requested = {normalize_text(value) for value in preferred_cuisines}
            if cuisine in requested:
                score += CURATED_RECIPE_CUISINE_REWARD

    return score


def top_candidates(
    frame: pd.DataFrame,
    limit: int,
    preferred_cuisines: List[str],
) -> pd.DataFrame:

    if frame.empty:
        return frame.copy()

    result = frame.copy()

    result["__candidate_score"] = result.apply(lambda row: candidate_score(row, preferred_cuisines,), axis=1,)

    # Preserve variety by first taking top records from food groups,
    # then filling remaining places globally.
    selected_indices = []

    if "food_group" in result.columns:
        per_group = max(3, min(15, limit // max(1, result["food_group"].nunique(),),),)

        for _, group_frame in result.groupby("food_group", dropna=False,):
            group_top = (group_frame .sort_values("__candidate_score", ascending=False,) .head(per_group))

            selected_indices.extend(group_top.index.tolist())

    selected_indices = list(dict.fromkeys(selected_indices))

    remaining = (result .drop(index=selected_indices, errors="ignore",) .sort_values("__candidate_score", ascending=False,))

    selected_indices.extend(remaining .head(max(0, limit - len(selected_indices),)) .index .tolist())

    final = (result.loc[selected_indices] .sort_values("__candidate_score", ascending=False,) .head(limit) .drop(columns=["__candidate_score",], errors="ignore",) .copy())

    return final.reset_index(drop=True,)


def meal_contains(
    frame: pd.DataFrame,
    meal: str,
) -> pd.Series:
    return (frame["meal_type"] .fillna("") .astype(str) .str.lower() .str.contains(meal, regex=False,))


def build_candidate_pools(
    filtered: pd.DataFrame,
    preferred_cuisines: List[str],
) -> Tuple[
    Dict[str, pd.DataFrame],
    pd.DataFrame,
    List[str],
]:

    notes = []

    non_dessert = filtered[~filtered["meal_type"] .fillna("") .astype(str) .str.lower() .str.contains("dessert", regex=False,)].copy()

    prepared = non_dessert[non_dessert["candidate_type"] .astype(str) .str.lower() .eq("prepared_dish")].copy()

    breakfast = non_dessert[meal_contains(non_dessert, "breakfast",)].copy()

    breakfast = breakfast[breakfast.apply(is_valid_breakfast_main, axis=1,)].copy()

    snack = non_dessert[meal_contains(non_dessert, "snack",)].copy()

    lunch = prepared[meal_contains(prepared, "lunch",)].copy()

    lunch = lunch[lunch.apply(is_valid_main_dish, axis=1,)].copy()

    supper = prepared[meal_contains(prepared, "supper",)].copy()

    supper = supper[supper.apply(is_valid_main_dish, axis=1,)].copy()

    dessert = filtered[meal_contains(filtered, "dessert",)].copy()

    sides = filtered[
        filtered[
            "candidate_type"
        ]
        .astype(str)
        .str.lower()
        .eq(
            "ingredient"
        )
        & filtered[
            "food_group"
        ].isin(
            SIDE_GROUPS
        )
        & ~meal_contains(
            filtered,
            "dessert",
        )
    ].copy()

    sides = sides[sides.apply(is_practical_supporting_food, axis=1,)].copy()

    directly_servable_sides = sides[~sides.apply(is_as_ingredient_record, axis=1,)].copy()

    if len(directly_servable_sides) >= 25:
        sides = directly_servable_sides

    pools = {
        "breakfast": top_candidates(
            breakfast,
            MAX_BREAKFAST_CANDIDATES,
            preferred_cuisines,
        ),
        "lunch": top_candidates(
            lunch,
            MAX_LUNCH_CANDIDATES,
            preferred_cuisines,
        ),
        "snack": top_candidates(
            snack,
            MAX_SNACK_CANDIDATES,
            preferred_cuisines,
        ),
        "supper": top_candidates(
            supper,
            MAX_SUPPER_CANDIDATES,
            preferred_cuisines,
        ),
        "dessert": top_candidates(
            dessert,
            MAX_DESSERT_CANDIDATES,
            preferred_cuisines,
        ),
    }

    sides = top_candidates(sides, MAX_SIDE_CANDIDATES, preferred_cuisines,)

    for role in ["breakfast", "lunch", "snack", "supper",]:
        if pools[role].empty:
            notes.append(f"No eligible {role} candidates remain after filtering.")

    return (pools, sides, notes,)


def build_curated_main_priority_pools(
    pools: Dict[str, pd.DataFrame],
) -> Tuple[
    Dict[str, pd.DataFrame],
    List[str],
]:
    """
    Build a first-pass pool that requires curated cuisine recipes for
    breakfast, lunch and supper where curated candidates are available.

    The caller can retry with the original pools if this stricter pass
    is infeasible, preserving USDA prepared dishes as a fallback.
    """

    priority_roles = {"breakfast", "lunch", "supper",}

    result = {}
    applied_roles = []

    for role, frame in pools.items():
        if (role in priority_roles and not frame.empty):
            curated = frame[frame.apply(is_curated_recipe, axis=1,)].copy()

            if not curated.empty:
                result[role] = (curated.reset_index(drop=True,))
                applied_roles.append(role)
                continue

        result[role] = frame.copy()

    return (result, applied_roles,)


def side_family(
    row,
) -> str:
    """
    Group visible supporting foods into broad presentation families
    for same-day variety rules.
    """
    group = normalize_text(row.get("food_group", "",))

    text = row_text(row)

    if group == "vegetables":
        if contains_any_term(text, {"kale"},):
            return "vegetable_kale"

        if contains_any_term(text, {"spinach"},):
            return "vegetable_spinach"

        if contains_any_term(text, {"carrot", "carrots"},):
            return "vegetable_carrot"

        if contains_any_term(text, {"pumpkin", "butternut"},):
            return "vegetable_pumpkin"

        return "vegetable_other"

    if group == "fruit":
        if contains_any_term(text, {"apple"},):
            return "fruit_apple"

        if contains_any_term(text, {"banana"},):
            return "fruit_banana"

        if contains_any_term(text, {"blueberry", "blueberries", "strawberry", "strawberries", "berries",},):
            return "fruit_berries"

        return "fruit_other"

    if group == "grains & staples":
        if is_cereal_supporting_food(row) or contains_any_term(text, {"oatmeal", "oats", "porridge",},):
            return "grain_cereal"

        return "grain_other"

    return ""


def is_cereal_based_main(
    row,
) -> bool:
    text = row_text(row)

    return contains_any_term(text, {"oatmeal", "oats", "porridge", "cereal", "bran flakes", "corn flakes", "granola", "muesli",},)

# LP HELPERS

def row_key(
    role: str,
    catalogue_id: str,
) -> str:
    safe_id = re.sub(r"[^A-Za-z0-9_]", "_", str(catalogue_id),)

    return (f"{role}_{safe_id}")


def nutrient_coefficient(
    row,
    column: str,
) -> float:
    return safe_float(row.get(column,), 0.0,) / 100.0


def solve_meal_lp(
    pools: Dict[str, pd.DataFrame],
    sides: pd.DataFrame,
    energy_req: float,
    protein_req: float,
    active_targets: Dict[str, dict],
    enforce_variety: bool = True,
) -> Tuple[
    Optional[dict],
    str,
]:

    required_roles = ["breakfast", "lunch", "snack", "supper",]

    if any(pools[role].empty for role in required_roles):
        return (None, "NoEligibleMealCandidates",)

    model = pulp.LpProblem("MaternalUSDA_MealPlan", pulp.LpMinimize,)

    meal_choice_vars = {}
    meal_gram_vars = {}
    meal_rows = {}

    # MAIN MEAL / SNACK / DESSERT VARIABLES

    for role, frame in pools.items():
        for _, row in frame.iterrows():
            catalogue_id = str(row["catalogue_id"])

            key = (role, catalogue_id,)

            var_name = row_key(role, catalogue_id,)

            chosen = pulp.LpVariable(f"choose_{var_name}", cat="Binary",)

            grams = pulp.LpVariable(f"grams_{var_name}", lowBound=0,)

            lower, upper, _, _ = role_gram_bounds(row, role,)

            model += (grams <= upper * chosen)

            model += (grams >= lower * chosen)

            meal_choice_vars[key] = chosen
            meal_gram_vars[key] = grams
            meal_rows[key] = row

    # Exactly one breakfast/lunch/snack/supper.
    for role in required_roles:
        role_keys = [key for key in meal_choice_vars if key[0] == role]

        model += (pulp.lpSum(meal_choice_vars[key] for key in role_keys) == 1), f"ExactlyOne_{role}"

    # Dessert optional, maximum one.
    dessert_keys = [key for key in meal_choice_vars if key[0] == "dessert"]

    if dessert_keys:
        model += (pulp.lpSum(meal_choice_vars[key] for key in dessert_keys) <= 1), "OptionalDessert"

    # Prevent the same prepared dish being selected for more than
    # one meal slot in the same day.
    all_meal_ids = set(catalogue_id for _, catalogue_id in meal_choice_vars.keys())

    for catalogue_id in all_meal_ids:
        matching = [key for key in meal_choice_vars if key[1] == catalogue_id]

        if len(matching) > 1:
            model += (pulp.lpSum(meal_choice_vars[key] for key in matching) <= 1)

    # SUPPORTING INGREDIENT VARIABLES

    side_choice_vars = {}
    side_gram_vars = {}
    side_rows = {}

    for _, row in sides.iterrows():
        catalogue_id = str(row["catalogue_id"])

        var_name = row_key("side", catalogue_id,)

        chosen = pulp.LpVariable(f"choose_{var_name}", cat="Binary",)

        grams = pulp.LpVariable(f"grams_{var_name}", lowBound=0,)

        lower, upper, _, _ = role_gram_bounds(row, "side",)

        model += (grams <= upper * chosen)

        model += (grams >= lower * chosen)

        side_choice_vars[catalogue_id] = chosen
        side_gram_vars[catalogue_id] = grams
        side_rows[catalogue_id] = row

    # DAILY VARIETY RULES

    processed_bar_meal_keys = [key for key, row in meal_rows.items() if is_processed_bar(row)]

    processed_bar_side_ids = [catalogue_id for catalogue_id, row in side_rows.items() if is_processed_bar(row)]

    if (processed_bar_meal_keys or processed_bar_side_ids):
        model += (
            pulp.lpSum(
                meal_choice_vars[key]
                for key
                in processed_bar_meal_keys
            )
            +
            pulp.lpSum(
                side_choice_vars[catalogue_id]
                for catalogue_id
                in processed_bar_side_ids
            )
            <= 1
        ), "MaximumOneProcessedBarPerDay"

    # Lunch and supper should not come from the same recognisable
    # main-dish family. This also means at most one sandwich-style
    # lunch/supper main per day.
    lunch_supper_families = {main_dish_family(row) for key, row in meal_rows.items() if (key[0] in {"lunch", "supper",} and main_dish_family(row) != "other")}

    for family in sorted(lunch_supper_families):
        family_keys = [key for key, row in meal_rows.items() if (key[0] in {"lunch", "supper",} and main_dish_family(row) == family)]

        roles_present = {key[0] for key in family_keys}

        if enforce_variety and len(roles_present) >= 2:
            safe_family = family.replace(" ", "_").replace("-", "_")
            model += (pulp.lpSum(meal_choice_vars[key] for key in family_keys) <= 1), f"DailyMainFamilyVariety_{safe_family}"

    if side_choice_vars:
        model += (pulp.lpSum(side_choice_vars.values()) <= MAX_SUPPORTING_ITEMS), "MaximumSupportingFoods"

        # Encourage a real daily plan rather than only four main records.
        model += (pulp.lpSum(side_choice_vars.values()) >= min(2, len(side_choice_vars),)), "MinimumSupportingFoods"

        vegetable_ids = [catalogue_id for catalogue_id, row in side_rows.items() if is_whole_vegetable_candidate(row)]

        if vegetable_ids:
            model += (pulp.lpSum(side_choice_vars[i] for i in vegetable_ids) >= 1), "IncludeWholeVegetable"

        fruit_ids = [catalogue_id for catalogue_id, row in side_rows.items() if str(row.get("food_group", "",)).strip() == "Fruit"]

        if fruit_ids:
            model += (pulp.lpSum(side_choice_vars[i] for i in fruit_ids) >= 1), "IncludeFruit"

    cereal_side_ids = [catalogue_id for catalogue_id, row in side_rows.items() if is_cereal_supporting_food(row)]

    if cereal_side_ids:
        model += (pulp.lpSum(side_choice_vars[catalogue_id] for catalogue_id in cereal_side_ids) <= 1), "MaximumOneStandaloneCerealSidePerDay"

    # Hard cereal realism rule. If breakfast or snack is already cereal/oat/
    # porridge based, an additional standalone cereal supporting food is not allowed.
    # This remains active even when cosmetic daily-variety rules are relaxed.
    cereal_main_keys = [key for key, row in meal_rows.items() if key[0] in {"breakfast", "snack"} and is_cereal_based_main(row)]
    grain_cereal_side_ids = [catalogue_id for catalogue_id, row in side_rows.items() if side_family(row) == "grain_cereal"]
    if cereal_main_keys and grain_cereal_side_ids:
        model += (
            pulp.lpSum(meal_choice_vars[key] for key in cereal_main_keys)
            + pulp.lpSum(side_choice_vars[catalogue_id] for catalogue_id in grain_cereal_side_ids)
            <= 1
        ), "NoExtraCerealSideWithCerealMain"

    # Avoid repeating the same obvious supporting-food family in one day
    # (for example raw kale plus cooked kale).
    family_to_ids = {}

    for catalogue_id, row in side_rows.items():
        family = side_family(row)

        if not family:
            continue

        family_to_ids.setdefault(family, [],).append(catalogue_id)

    if enforce_variety:
        for family, ids in family_to_ids.items():
            if len(ids) < 2: continue
            safe_family = re.sub(r"[^A-Za-z0-9_]", "_", family)
            model += (pulp.lpSum(side_choice_vars[catalogue_id] for catalogue_id in ids) <= 1), f"DailySideFamilyVariety_{safe_family}"

    # TOTAL NUTRIENT EXPRESSIONS

    def total_expression(
        nutrient_column: str,
    ):
        meal_part = pulp.lpSum(meal_gram_vars[key] * nutrient_coefficient(meal_rows[key], nutrient_column,) for key in meal_gram_vars)

        side_part = pulp.lpSum(side_gram_vars[catalogue_id] * nutrient_coefficient(side_rows[catalogue_id], nutrient_column,) for catalogue_id in side_gram_vars)

        return (meal_part + side_part)

    energy_actual = total_expression("energy_100g")

    protein_actual = total_expression("proteins_100g")

    sodium_actual = total_expression("sodium_100g")

    fat_actual = total_expression("fat_100g")

    # GAM TARGET BANDS

    model += (energy_actual >= energy_req * ENERGY_MIN_RATIO), "Energy_GAM_Lower"

    model += (energy_actual <= energy_req * ENERGY_MAX_RATIO), "Energy_GAM_Upper"

    model += (protein_actual >= protein_req * PROTEIN_MIN_RATIO), "Protein_GAM_Lower"

    model += (protein_actual <= protein_req * PROTEIN_MAX_RATIO), "Protein_GAM_Upper"

    # MICRONUTRIENTS

    nutrient_expressions = {}

    for column, config in active_targets.items():
        expression = total_expression(column)

        nutrient_expressions[column] = expression

        safe_name = re.sub(r"[^A-Za-z0-9_]", "_", column,)

        model += (expression >= float(config["value"])), f"Minimum_{safe_name}"

    # Vitamin D food-first soft target.
    # The optimiser is rewarded for reaching the verified 10 µg/day target,
    # but a shortfall cannot make the whole daily plan infeasible.
    vitamin_d_target = float(BASE_MICRONUTRIENT_TARGETS["vitamin-d_100g"]["value"])
    vitamin_d_actual_expr = total_expression("vitamin-d_100g")
    vitamin_d_shortfall = pulp.LpVariable("Vitamin_D_Soft_Shortfall", lowBound=0)
    model += vitamin_d_shortfall >= vitamin_d_target - vitamin_d_actual_expr, "Vitamin_D_Soft_Target"
    fibre_actual_expr = total_expression("fiber_100g")
    fibre_excess = pulp.LpVariable("Fibre_Soft_Excess", lowBound=0)
    model += fibre_excess >= fibre_actual_expr - FIBRE_SOFT_THRESHOLD_G, "Fibre_Soft_Excess_Definition"

    # MEAL ENERGY REALISM

    for role in ["breakfast", "lunch", "snack", "supper",]:
        role_energy = pulp.lpSum(meal_gram_vars[key] * nutrient_coefficient(meal_rows[key], "energy_100g",) for key in meal_gram_vars if key[0] == role)

        min_ratio, max_ratio = (MEAL_ENERGY_BANDS[role])

        model += (role_energy >= energy_req * min_ratio), f"{role}_Energy_Min"

        model += (role_energy <= energy_req * max_ratio), f"{role}_Energy_Max"

    # MAIN-MEAL SUBSTANCE REQUIREMENTS
    # These prevent technically valid but implausible mains such as
    # syrup or shells from satisfying an entire meal slot.

    for role in ["breakfast", "lunch", "supper",]:
        role_energy = pulp.lpSum(meal_gram_vars[key] * nutrient_coefficient(meal_rows[key], "energy_100g",) for key in meal_gram_vars if key[0] == role)

        role_protein = pulp.lpSum(meal_gram_vars[key] * nutrient_coefficient(meal_rows[key], "proteins_100g",) for key in meal_gram_vars if key[0] == role)

        model += (role_energy >= MAIN_MIN_ENERGY_KCAL[role]), f"{role}_Minimum_Main_Energy"

        model += (role_protein >= MAIN_MIN_PROTEIN_G[role]), f"{role}_Minimum_Main_Protein"

    if dessert_keys:
        dessert_energy = pulp.lpSum(meal_gram_vars[key] * nutrient_coefficient(meal_rows[key], "energy_100g",) for key in dessert_keys)

        model += (dessert_energy <= energy_req * MEAL_ENERGY_BANDS["dessert"][1]), "Dessert_Energy_Max"

    # OBJECTIVE

    energy_under = pulp.LpVariable("energy_under_target", lowBound=0,)

    energy_over = pulp.LpVariable("energy_over_target", lowBound=0,)

    protein_under = pulp.LpVariable("protein_under_target", lowBound=0,)

    protein_over = pulp.LpVariable("protein_over_target", lowBound=0,)

    model += (energy_actual + energy_under - energy_over == energy_req), "EnergyDeviation"

    model += (protein_actual + protein_under - protein_over == protein_req), "ProteinDeviation"

    limit_penalty_terms = []
    sodium_penalty_terms = []
    dense_penalty_terms = []
    quality_penalty_terms = []
    processed_bar_penalty_terms = []
    processed_snack_penalty_terms = []
    ingredient_record_penalty_terms = []
    curated_recipe_reward_terms = []

    for key, chosen in meal_choice_vars.items():
        row = meal_rows[key]

        if (str(row.get("lp_use_class", "",)).upper() == "LIMIT"):
            limit_penalty_terms.append(chosen)

        if to_bool(row.get("high_sodium_flag", False,)):
            sodium_penalty_terms.append(chosen)

        if to_bool(row.get("very_energy_dense_flag", False,)):
            dense_penalty_terms.append(chosen)

        quality_penalty = food_quality_penalty(row)

        if quality_penalty > 0:
            quality_penalty_terms.append(quality_penalty * chosen)

        if is_processed_bar(row):
            processed_bar_penalty_terms.append(PROCESSED_BAR_SELECTION_PENALTY * chosen)

        if is_processed_snack_side(row):
            processed_snack_penalty_terms.append(PROCESSED_SNACK_SIDE_PENALTY * chosen)

        if is_as_ingredient_record(row):
            ingredient_record_penalty_terms.append(AS_INGREDIENT_SELECTION_PENALTY * chosen)

        if is_curated_recipe(row):
            curated_recipe_reward_terms.append(CURATED_RECIPE_SELECTION_REWARD * chosen)

    for catalogue_id, chosen in side_choice_vars.items():
        row = side_rows[catalogue_id]

        if (str(row.get("lp_use_class", "",)).upper() == "LIMIT"):
            limit_penalty_terms.append(chosen)

        if to_bool(row.get("high_sodium_flag", False,)):
            sodium_penalty_terms.append(chosen)

        if to_bool(row.get("very_energy_dense_flag", False,)):
            dense_penalty_terms.append(chosen)

        quality_penalty = food_quality_penalty(row)

        if quality_penalty > 0:
            quality_penalty_terms.append(quality_penalty * chosen)

        if is_processed_bar(row):
            processed_bar_penalty_terms.append(PROCESSED_BAR_SELECTION_PENALTY * chosen)

        if is_processed_snack_side(row):
            processed_snack_penalty_terms.append(PROCESSED_SNACK_SIDE_PENALTY * chosen)

        if is_as_ingredient_record(row):
            ingredient_record_penalty_terms.append(AS_INGREDIENT_SELECTION_PENALTY * chosen)

    objective = (100.0 * (energy_under + energy_over) / max(energy_req, 1.0,))

    objective += (180.0 * (protein_under + protein_over) / max(protein_req, 1.0,))

    if limit_penalty_terms:
        objective += (LIMIT_SELECTION_PENALTY * pulp.lpSum(limit_penalty_terms))

    if sodium_penalty_terms:
        objective += (HIGH_SODIUM_SELECTION_PENALTY * pulp.lpSum(sodium_penalty_terms))

    if dense_penalty_terms:
        objective += (ENERGY_DENSE_SELECTION_PENALTY * pulp.lpSum(dense_penalty_terms))

    if dessert_keys:
        objective += (DESSERT_SELECTION_PENALTY * pulp.lpSum(meal_choice_vars[key] for key in dessert_keys))

    # Soft food-quality preferences. These do not add new hard
    # nutrition requirements; they simply guide the solver toward
    # lower-sodium, lower-fat and less convenience-heavy choices.
    objective += (SODIUM_OBJECTIVE_WEIGHT * sodium_actual)

    objective += (FAT_OBJECTIVE_WEIGHT * fat_actual)

    # Strong food-first preference for Vitamin D. A full 10 µg shortfall costs
    # 35 objective points; the solver can still accept it when satisfying the
    # target would require a worse or impractical overall food plan.
    objective += (VITAMIN_D_SOFT_TARGET_WEIGHT / max(vitamin_d_target, 1.0,)) * vitamin_d_shortfall

    objective += (FIBRE_EXCESS_PENALTY_WEIGHT / FIBRE_SOFT_THRESHOLD_G) * fibre_excess

    if quality_penalty_terms:
        objective += pulp.lpSum(quality_penalty_terms)

    if processed_bar_penalty_terms:
        objective += pulp.lpSum(processed_bar_penalty_terms)

    if processed_snack_penalty_terms:
        objective += pulp.lpSum(processed_snack_penalty_terms)

    if ingredient_record_penalty_terms:
        objective += pulp.lpSum(ingredient_record_penalty_terms)

    if curated_recipe_reward_terms:
        objective -= pulp.lpSum(curated_recipe_reward_terms)

    if side_choice_vars:
        objective += (0.5 * pulp.lpSum(side_choice_vars.values()))

        objective += (0.001 * pulp.lpSum(side_gram_vars.values()))

    model += objective

    # SOLVE

    solver = pulp.PULP_CBC_CMD(msg=False, timeLimit=SOLVER_TIME_LIMIT_SECONDS,)

    model.solve(solver)

    status = pulp.LpStatus[model.status]

    if status not in {"Optimal", "Feasible",}:
        return (None, status,)

    selected_meals = {}

    for key, chosen in meal_choice_vars.items():
        chosen_value = safe_float(chosen.value(), 0.0,)

        grams_value = safe_float(meal_gram_vars[key].value(), 0.0,)

        if (chosen_value > 0.5 and grams_value > 0.01):
            role, catalogue_id = key

            selected_meals[role] = {"catalogue_id": catalogue_id, "grams": grams_value, "row": meal_rows[key],}

    selected_sides = []

    for catalogue_id, chosen in side_choice_vars.items():
        chosen_value = safe_float(chosen.value(), 0.0,)

        grams_value = safe_float(side_gram_vars[catalogue_id].value(), 0.0,)

        if (chosen_value > 0.5 and grams_value > 0.01):
            selected_sides.append({"catalogue_id": catalogue_id, "grams": grams_value, "row": side_rows[catalogue_id],})

    return ({"selected_meals": selected_meals, "selected_sides": selected_sides,}, status,)




def vitamin_d_candidate_diagnostics(
    pools: Dict[str, pd.DataFrame],
    sides: pd.DataFrame,
    top_n: int = 12,
) -> List[str]:
    """
    Report the strongest Vitamin-D-containing foods that actually survived
    filtering and candidate shortlisting for the current request.

    Diagnostic only: this does not change the optimisation.
    """
    nutrient_col = "vitamin-d_100g"
    records: List[dict] = []

    def collect(frame: pd.DataFrame, role: str) -> None:
        if frame is None or frame.empty or nutrient_col not in frame.columns:
            return

        for _, row in frame.iterrows():
            vitd = safe_float(row.get(nutrient_col, 0.0), 0.0)
            if vitd <= 0:
                continue

            name = str(row.get("display_name", row.get("description", ""))).strip()
            if not name:
                continue

            serving_g = safe_float(row.get("serving_size_g", 0.0), 0.0)

            # Use the same broad maximum serving logic as the LP where possible.
            # For mains, the LP allows up to 1.5 defined servings.
            # For supporting foods, use the subtype maximum gram bound.
            if role == "supporting side":
                subtype = side_food_subtype(row)
                bounds = SIDE_SUBTYPE_GRAM_BOUNDS.get(subtype)
                if bounds:
                    max_g = float(bounds[1])
                else:
                    max_g = max(serving_g, 100.0)
            else:
                max_g = serving_g * 1.5 if serving_g > 0 else 0.0

            max_vitd = vitd * max_g / 100.0 if max_g > 0 else 0.0

            records.append({"name": name, "role": role, "vitd": vitd, "serving_g": serving_g, "max_g": max_g, "max_vitd": max_vitd,})

    for role in ["breakfast", "lunch", "snack", "supper", "dessert"]:
        collect(pools.get(role, pd.DataFrame()), role)

    collect(sides, "supporting side")

    if not records:
        return ["Vitamin D diagnostic: No shortlisted food or recipe with a positive " "Vitamin D value remains after the current filters and candidate selection."]

    # Deduplicate the same food/role combination, then rank by maximum possible
    # Vitamin D contribution under the diagnostic portion estimate.
    dedup = {}
    for rec in records:
        key = (rec["name"].lower(), rec["role"])
        if key not in dedup or rec["max_vitd"] > dedup[key]["max_vitd"]:
            dedup[key] = rec

    ranked = sorted(dedup.values(), key=lambda x: (x["max_vitd"], x["vitd"]), reverse=True,)[:top_n]

    notes = ["Vitamin D diagnostic: strongest surviving Vitamin-D-containing " "candidates after filtering and shortlisting:"]

    for rec in ranked:
        serving_text = (f"{rec['serving_g']:.0f} g defined serving" if rec["serving_g"] > 0 else "no reliable defined serving")
        max_text = (f"{rec['max_g']:.0f} g diagnostic max" if rec["max_g"] > 0 else "diagnostic max unavailable")

        notes.append(
            "Vitamin D candidate: "
            f"{rec['name']} | eligible as {rec['role']} | "
            f"{rec['vitd']:.2f} µg/100 g | {serving_text} | "
            f"{max_text} | estimated maximum contribution "
            f"{rec['max_vitd']:.2f} µg."
        )

    best = ranked[0]
    notes.append(
        "Vitamin D diagnostic summary: the strongest single surviving candidate "
        f"is '{best['name']}', with an estimated maximum contribution of "
        f"{best['max_vitd']:.2f} µg toward the 10 µg/day target."
    )

    return notes


def diagnose_infeasible_plan(
    pools: Dict[str, pd.DataFrame],
    sides: pd.DataFrame,
    energy_req: float,
    protein_req: float,
    active_targets: Dict[str, dict],
) -> List[str]:
    """
    Diagnostic-only analysis used after the normal LP has already failed.

    IMPORTANT:
    - This function does NOT change the plan returned to the user.
    - It does NOT weaken allergy, diet, exclusion, portion, meal-structure,
      energy or protein rules in the real optimisation.
    - It temporarily removes micronutrient constraints in diagnostic solves
      only, so we can identify where infeasibility is coming from.
    """

    messages: List[str] = []

    # Candidate counts make pool scarcity visible immediately.
    for role in ["breakfast", "lunch", "snack", "supper", "dessert",]:
        count = int(len(pools.get(role, pd.DataFrame(),)))
        messages.append(f"Diagnostic: {role.capitalize()} candidates available = {count}.")

    messages.append(f"Diagnostic: Supporting-food candidates available = {int(len(sides))}.")

    # First test whether the problem is feasible when ONLY the hard
    # micronutrient constraints are removed.
    base_solution, base_status = solve_meal_lp(pools, sides, energy_req, protein_req, {}, enforce_variety=False,)

    if base_solution is None:
        messages.append(
            "Diagnostic result: The model is still infeasible even when all "
            "active micronutrient minimums are removed. The conflict is "
            "therefore in the energy/protein range, meal structure, candidate "
            "availability, portion limits, supporting-food rules, or a "
            "combination of those constraints."
        )
        messages.append(f"Diagnostic base solver status: {base_status}.")
        return messages

    messages.append(
        "Diagnostic result: Energy/protein, meal structure, candidate "
        "availability and portion rules are feasible when the active "
        "micronutrient minimums are removed. The infeasibility is therefore "
        "caused by one or more active micronutrient constraints in combination "
        "with the current food pool."
    )

    if not active_targets:
        messages.append("Diagnostic note: No active micronutrient constraints were present, " "so the remaining conflict requires deeper structural diagnosis.")
        return messages

    individually_blocking: List[str] = []

    # Remove one micronutrient at a time. If that single removal restores
    # feasibility, that nutrient is an individually identifiable blocker.
    for column, config in active_targets.items():
        reduced_targets = {key: value for key, value in active_targets.items() if key != column}

        test_solution, _ = solve_meal_lp(pools, sides, energy_req, protein_req, reduced_targets, enforce_variety=False,)

        if test_solution is not None:
            label = str(config.get("label", column,))
            target = safe_float(config.get("value", 0.0,), 0.0,)
            unit = str(config.get("unit", "",)).strip()

            target_text = (f" (target {target:g} {unit})" if target > 0 else "")

            individually_blocking.append(f"{label}{target_text}")

    if individually_blocking:
        messages.append("Diagnostic micronutrient blocker(s): Removing the following " "constraint individually restores feasibility: " + "; ".join(individually_blocking) + ".")

        vitamin_d_blocking = any(str(item).lower().startswith("vitamin d") for item in individually_blocking)
        if vitamin_d_blocking:
            messages.extend(vitamin_d_candidate_diagnostics(pools, sides, top_n=12,))
    else:
        messages.append(
            "Diagnostic micronutrient result: No single micronutrient "
            "constraint restores feasibility when removed by itself. The "
            "failure is caused by the combined effect of multiple active "
            "micronutrient targets with the current foods and portion limits."
        )

    return messages

# POST-SOLVE NUTRIENT CALCULATION

def selected_entries(
    solution: dict,
) -> List[dict]:

    entries = []

    for role, item in (solution["selected_meals"].items()):
        entries.append({"role": role, "grams": item["grams"], "row": item["row"],})

    for item in solution["selected_sides"]:
        entries.append({"role": "side", "grams": item["grams"], "row": item["row"],})

    return entries


def calculate_actual_nutrients(
    solution: dict,
    target_columns: List[str],
) -> Dict[str, float]:

    totals = {"energy_kcal": 0.0, "protein_g": 0.0,}

    for column in (list(INFORMATION_NUTRIENTS.keys()) + target_columns):
        totals[column] = 0.0

    for entry in selected_entries(solution):
        row = entry["row"]
        grams = entry["grams"]
        multiplier = (grams / 100.0)

        totals["energy_kcal"] += (safe_float(row.get("energy_100g"), 0.0,) * multiplier)

        totals["protein_g"] += (safe_float(row.get("proteins_100g"), 0.0,) * multiplier)

        for column in (list(INFORMATION_NUTRIENTS.keys()) + target_columns):
            totals[column] += (safe_float(row.get(column), 0.0,) * multiplier)

    return totals

# RECOMMENDATION / MEAL OUTPUT

def friendly_fraction(value: float) -> str:
    """
    Convert a serving multiplier into a mother-friendly fraction.
    The nutrient calculation still uses the exact gram value.
    """
    if value <= 0:
        return ""

    # Snap to the nearest quarter-serving for display only.
    snapped = round(value * 4.0) / 4.0
    snapped = max(0.25, min(snapped, 3.0))

    whole = int(snapped)
    fraction = round(snapped - whole, 2)

    frac_map = {0.0: "", 0.25: "¼", 0.5: "½", 0.75: "¾",}

    frac = frac_map.get(fraction, "")

    if whole == 0:
        return frac

    if not frac:
        return str(whole)

    return f"{whole}{frac}"


def singular_serving_description(description: str) -> str:
    """
    Remove a leading '1' from descriptions such as '1 medium bowl'
    so a multiplier can be placed in front cleanly.

    Missing pandas/object values such as 'nan', 'none' and 'null'
    are treated as empty descriptions so the practical fallback
    household measure can be used instead.
    """
    text = str(description or "").strip()

    if (not text or text.lower() in {"nan", "none", "null", "quantity not specified", "not specified", "unknown",}):
        return ""

    text = re.sub(r"^\s*1(?:\.0+)?\s+", "", text, flags=re.I,)

    return text.strip()


def snapped_serving_value(value: float) -> float:
    if value <= 0:
        return 1.0

    snapped = round(value * 4.0) / 4.0
    return max(0.25, min(snapped, 3.0))


def curated_fallback_description(
    row,
    role: str,
) -> str:
    """
    Replace generic curated-recipe wording with a practical
    meal-specific household description for reporting only.
    """
    text = row_text(row)

    if role == "breakfast":
        if contains_any_term(text, {"oatmeal", "oats", "porridge", "cereal", "yogurt", "smoothie",},):
            return "medium bowl"

        return "breakfast plate"

    if role == "lunch":
        if contains_any_term(text, {"stew", "curry", "pasta", "rice", "bowl",},):
            return "medium bowl"

        return "plate"

    if role == "snack":
        if contains_any_term(text, {"yogurt", "fruit", "oat", "mango", "banana", "apple",},):
            return "small bowl"

        return "small snack plate"

    if role == "supper":
        if contains_any_term(text, {"stew", "curry", "pasta", "rice", "bowl",},):
            return "medium bowl"

        return "plate"

    if role == "dessert":
        return "small dessert bowl"

    return "serving"


def clean_display_food_name(name: str) -> str:
    """
    Small display-only typo cleanup. Nutrient identity and catalogue
    references remain unchanged.
    """
    value = str(name or "").strip()

    replacements = {"Bluberries": "Blueberries", "bluberries": "blueberries",}

    for wrong, correct in replacements.items():
        value = value.replace(wrong, correct,)

    return value


def fallback_household_description(
    row,
    role: str,
) -> str:
    """
    Practical reporting fallback used only when the source has no
    useful household description.
    """
    group = normalize_text(row.get("food_group", "",))

    text = row_text(row)

    if contains_any_term(text, PLANT_MILK_PHRASES,):
        return "cup"

    if group == "fruit":
        if contains_term(text, "banana"):
            return "medium banana"
        if contains_term(text, "apple"):
            return "medium apple"
        if contains_term(text, "orange"):
            return "medium orange"
        if contains_any_term(text, {"berry", "berries", "blackberry", "blackberries", "blueberry", "blueberries", "strawberry", "strawberries", "raspberry", "raspberries",},):
            return "cup"
        return "piece or cup"

    if group == "vegetables":
        return "cup"

    # Recognisable household foods take priority over potentially
    # imperfect USDA food-group metadata.
    if contains_any_term(text, {"milk", "kefir",},) and not contains_any_term(text, PLANT_MILK_PHRASES,):
        return "cup"

    if contains_any_term(text, {"yogurt", "yoghurt",},):
        return "small bowl"

    if group == "dairy":
        return "serving"

    if group == "nuts & seeds":
        return "small handful"

    if group == "eggs":
        return "large egg"

    if contains_any_term(text, {"muffin", "muffins",},):
        return "small muffin"

    if contains_any_term(text, {"pretzel", "pretzels", "cracker", "crackers", "crispbread",},):
        return "small handful"

    if group == "grains & staples":
        if contains_any_term(text, {"rice", "quinoa", "couscous", "pasta"}):
            return "cooked cup"
        if contains_any_term(text, {"bread", "toast"}):
            return "slice"
        return "serving"

    if group == "legumes & pulses":
        return "cooked cup"

    if role in {"breakfast", "lunch", "supper"}:
        return "bowl or plate"

    if role == "snack":
        return "snack serving"

    if role == "dessert":
        return "small dessert bowl"

    return "serving"


def practical_portion_text(
    row,
    grams: float,
    role: str,
    serving: float,
    source_description: str,
    serving_source: str,
) -> str:
    """
    Build a user-facing portion such as:
        '1 medium bowl (about 335 g)'
        '½ cup (about 75 g)'
        '1 medium banana (about 120 g)'

    Exact grams remain the source of truth for all LP/nutrient calculations.
    """
    servings = (grams / serving if serving and serving > 0 else 1.0)

    multiplier = friendly_fraction(servings) or "1"

    if serving_source in REPORTING_FALLBACK_DESCRIPTIONS:
        base_description = REPORTING_FALLBACK_DESCRIPTIONS[serving_source]
    else:
        base_description = singular_serving_description(source_description)

        if (is_curated_recipe(row) and (not base_description or normalize_text(base_description) in {"curated recipe serving", "recipe serving", "serving",})):
            base_description = curated_fallback_description(row, role,)

        source_description_normal = normalize_text(base_description)

        if contains_any_term(
            source_description_normal,
            {
                "blackberry",
                "blueberry",
                "strawberry",
                "raspberry",
                "miniature",
                "bite size",
                "crispbread",
                "cracker",
                "quantity not specified",
            },
        ):
            base_description = fallback_household_description(row, role,)

        if not base_description:
            base_description = fallback_household_description(row, role,)

    # Lightweight pluralisation for common household measures.
    plural_map = {
        "cup": "cups",
        "tablespoon": "tablespoons",
        "teaspoon": "teaspoons",
        "slice": "slices",
        "piece": "pieces",
        "medium bowl": "medium bowls",
        "large bowl": "large bowls",
        "small bowl": "small bowls",
        "plate": "plates",
        "breakfast plate": "breakfast plates",
        "small snack plate": "small snack plates",
        "small muffin": "small muffins",
        "small handful": "small handfuls",
        "large egg": "large eggs",
        "serving": "servings",
    }

    display_description = base_description
    display_servings = snapped_serving_value(servings)

    if display_servings > 1.0:
        display_description = plural_map.get(base_description, base_description,)

    return (f"{multiplier} {display_description} " f"(about {round(grams):.0f} g)")


def recommendation_item(
    row,
    grams: float,
    role: str,
    component_role: str,
) -> dict:

    reporting_role = (role if component_role == "main" and role in MEAL_SERVING_RANGES else "side")

    serving, serving_source = reporting_serving_grams(row, reporting_role,)

    servings = (grams / serving if serving > 0 else None)

    source_description = str(row.get("serving_description", "",)).strip()

    display_serving = practical_portion_text(row, grams, role, serving, source_description, serving_source,)

    display_name = clean_display_food_name(row.get("display_name", row.get("description", "Food",),))

    return {
        "catalogue_id": str(
            row.get(
                "catalogue_id",
                "",
            )
        ),
        "fdc_id": str(
            row.get(
                "fdc_id",
                "",
            )
        ),
        "food_name": display_name,
        "name": display_name,
        "food_item": display_name,
        "group": str(
            row.get(
                "food_group",
                "Other",
            )
        ).strip(),
        "meal": role,
        "component_role": component_role,
        "grams": round(
            grams,
            1,
        ),
        "serving_size_g": round(
            serving,
            1,
        ),
        "serving_grams": round(
            serving,
            1,
        ),
        "servings": (
            round(
                servings,
                2,
            )
            if servings is not None
            else None
        ),
        "serving_source": (
            "usda_household_portion"
            if serving_source
            == "USDA household portion"
            else serving_source
        ),
        # Mother-friendly serving text used by the frontend.
        "serving_description": display_serving,
        "display_serving": display_serving,
        "portion_text": display_serving,

        # Preserve the original USDA/curated description for audit purposes.
        "source_serving_description": (
            source_description
            if source_description
            else None
        ),
        "source": str(
            row.get(
                "source",
                "",
            )
        ),
        "is_curated_recipe": bool(
            is_curated_recipe(
                row
            )
        ),
        "ingredient_names": str(
            row.get(
                "ingredient_names",
                "",
            )
        ).strip(),
        "ingredient_keys": str(
            row.get(
                "ingredient_keys",
                "",
            )
        ).strip(),
        "candidate_type": str(
            row.get(
                "candidate_type",
                "",
            )
        ),
        "cuisine": str(
            row.get(
                "cuisine",
                "",
            )
        ),
        "lp_use_class": str(
            row.get(
                "lp_use_class",
                "",
            )
        ),
    }


def assign_side_to_meal(
    row,
    meal_counts: Dict[str, int],
) -> str:

    if is_breakfast_or_snack_side(row):
        options = ["breakfast", "snack",]

    elif is_lunch_or_supper_side(row):
        options = ["lunch", "supper",]

    else:
        # Conservative fallback.
        options = ["snack", "lunch",]

    return min(options, key=lambda meal: meal_counts.get(meal, 0,),)


def build_meal_plan(
    solution: dict,
) -> Tuple[
    Dict[str, List[dict]],
    List[dict],
]:

    meal_plan = {"Breakfast": [], "Lunch": [], "Snack": [], "Supper": [], "Dessert": [],}

    label_map = {"breakfast": "Breakfast", "lunch": "Lunch", "snack": "Snack", "supper": "Supper", "dessert": "Dessert",}

    flat = []

    for role in ["breakfast", "lunch", "snack", "supper", "dessert",]:
        item = (solution["selected_meals"].get(role))

        if not item:
            continue

        output_item = recommendation_item(item["row"], item["grams"], role, "main",)

        meal_plan[label_map[role]].append(output_item)

        flat.append(output_item)

    meal_counts = {"breakfast": len(meal_plan["Breakfast"]), "lunch": len(meal_plan["Lunch"]), "snack": len(meal_plan["Snack"]), "supper": len(meal_plan["Supper"]),}

    for item in solution["selected_sides"]:
        role = assign_side_to_meal(item["row"], meal_counts,)

        meal_counts[role] += 1

        output_item = recommendation_item(item["row"], item["grams"], role, "supporting",)

        meal_plan[label_map[role]].append(output_item)

        flat.append(output_item)

    # Do not show an empty dessert heading in the current frontend.
    if not meal_plan["Dessert"]:
        del meal_plan["Dessert"]

    return (meal_plan, flat,)

# NUTRIENT REPORT

def build_additional_attention(actual: Dict[str, float], all_targets: Dict[str, dict], diets: List[str], active_targets: Dict[str, dict]) -> List[dict]:
    """Mother-facing guidance for intentionally non-hard nutrient targets."""
    attention = []
    vegan = "vegan" in normalized_list(diets)

    guidance = {
        "vitamin-d_100g": {
            "label": "Vitamin D",
            "enabled": True,
            "message": "This food plan provides less Vitamin D than the 10 µg/day planning target. Vitamin D can be difficult to obtain from food alone. Consider Vitamin D-fortified foods and discuss whether a Vitamin D supplement is appropriate for you with a doctor, dietitian, nurse or other qualified healthcare professional."
        },
        "vitamin-b12_100g": {
            "label": "Vitamin B12",
            "enabled": vegan or "vitamin-b12_100g" not in active_targets,
            "message": "This meal plan did not provide enough vitamin B12 to meet the planning target. Reliable food sources of vitamin B12 may be limited by the selected diet and food restrictions. Consider suitable vitamin B12-fortified foods and discuss whether a vitamin B12 supplement is appropriate for you with a doctor, dietitian, nurse or other qualified healthcare professional."
        }
    }

    for column, config in guidance.items():
        target_cfg = all_targets.get(column)
        if not config["enabled"] or not target_cfg or column in active_targets: continue
        target, actual_value = safe_float(target_cfg.get("value"), 0.0), safe_float(actual.get(column), 0.0)
        if target > 0 and actual_value < target:
            attention.append({"key": column, "label": config["label"], "actual": round(actual_value, 2), "target": round(target, 2), "unit": target_cfg.get("unit", ""), "status": "attention_needed", "message": config["message"]})
    return attention


def nutrient_status(
    actual: float,
    target: float,
    tolerance: float = 0.01,
) -> str:

    if target <= 0:
        return "not_applicable"

    if (actual >= target * (1.0 - tolerance)):
        return "met"

    return "below_target"


def build_nutrient_report(
    actual: Dict[str, float],
    energy_req: float,
    protein_req: float,
    all_targets: Dict[str, dict],
    active_targets: Dict[str, dict],
) -> List[dict]:

    report = [
        {
            "key": "energy",
            "label": "Energy",
            "unit": "kcal",
            "target": round(
                energy_req,
                1,
            ),
            "actual": round(
                actual.get(
                    "energy_kcal",
                    0.0,
                ),
                1,
            ),
            "status": nutrient_status(
                actual.get(
                    "energy_kcal",
                    0.0,
                ),
                energy_req,
                0.02,
            ),
            "reference_type": "GAM",
            "age_condition": "Personalised model estimate",
        },
        {
            "key": "protein",
            "label": "Protein",
            "unit": "g",
            "target": round(
                protein_req,
                1,
            ),
            "actual": round(
                actual.get(
                    "protein_g",
                    0.0,
                ),
                1,
            ),
            "status": nutrient_status(
                actual.get(
                    "protein_g",
                    0.0,
                ),
                protein_req,
                0.05,
            ),
            "reference_type": "GAM",
            "age_condition": "Personalised model estimate",
        },
    ]

    for column, config in (all_targets.items()):
        value = actual.get(column, 0.0,)

        if column in active_targets:
            target = float(config["value"])

            status = nutrient_status(value, target, 0.01,)

        else:
            target = float(config["value"])

            if column in {"vitamin-b12_100g", "vitamin-d_100g"} and value < target:
                status = "attention_needed"
            elif column == "vitamin-d_100g" and value >= target:
                status = "met"
            else:
                status = "report_only"

        report.append({
            "key": column,
            "label": config[
                "label"
            ],
            "unit": config[
                "unit"
            ],
            "target": (
                round(
                    target,
                    2,
                )
                if target
                is not None
                else None
            ),
            "actual": round(
                value,
                2,
            ),
            "status": status,
            "reference_type": config.get(
                "reference_type",
                "",
            ),
            "age_condition": config.get(
                "age_condition",
                "",
            ),
            "source": config.get(
                "source",
                "",
            ),
        })

    for column, (label, unit,) in INFORMATION_NUTRIENTS.items():
        report.append({
            "key": column,
            "label": label,
            "unit": unit,
            "target": None,
            "actual": round(
                actual.get(
                    column,
                    0.0,
                ),
                2,
            ),
            "status": "information_only",
            "reference_type": "",
            "age_condition": "",
        })

    # Iodine is explicitly represented as not assessed.
    report.append({
        "key": "iodine",
        "label": "Iodine",
        "unit": "µg",
        "target": None,
        "actual": None,
        "status": "not_verifiable",
        "reference_type": "",
        "age_condition": "",
        "note": (
            "Iodine adequacy was not assessed because sufficiently "
            "complete iodine composition data were unavailable in "
            "the current food dataset."
        ),
    })

    return report

# FINAL SAFETY CHECK

def final_safety_check(
    selected_rows: List[dict],
    allergies: List[str],
    excluded_foods: List[str],
    diets: Optional[List[str]] = None,
) -> dict:

    allergen_violations = []
    exclusion_violations = []
    diet_violations = []

    normal_allergies = normalized_list(allergies)

    normal_exclusions = normalized_list(excluded_foods)

    for item in selected_rows:
        searchable = " ".join([normalize_text(item.get("food_name", "",)), normalize_text(item.get("ingredient_names", "",)), normalize_text(item.get("ingredient_keys", "",)),])

        normal_diets = set(normalized_list(diets or []))
        if "vegan" in normal_diets:
            pseudo_row = {
                "display_name": item.get("food_name", ""),
                "description": item.get("food_name", ""),
                "ingredient_names": item.get("ingredient_names", ""),
                "ingredient_keys": item.get("ingredient_keys", ""),
                "food_group": item.get("group", ""),
            }
            if has_vegan_animal_conflict(pseudo_row):
                diet_violations.append({"food": item.get("food_name"), "diet": "vegan",})
        elif "vegetarian" in normal_diets:
            pseudo_row = {
                "display_name": item.get("food_name", ""),
                "description": item.get("food_name", ""),
                "ingredient_names": item.get("ingredient_names", ""),
                "ingredient_keys": item.get("ingredient_keys", ""),
                "food_group": item.get("group", ""),
            }
            if has_vegetarian_animal_conflict(pseudo_row):
                diet_violations.append({"food": item.get("food_name"), "diet": "vegetarian",})

        # Excluded-food re-check mirrors the pre-filter, including semantic
        # aliases for USDA prepared dishes such as biryani when rice is excluded.
        safety_row = {"display_name": item.get("food_name", ""), "description": item.get("food_name", ""), "ingredient_names": item.get("ingredient_names", ""), "ingredient_keys": item.get("ingredient_keys", "")}
        for term in normal_exclusions:
            if excluded_food_matches(safety_row, term):
                exclusion_violations.append({"food": item.get("food_name"), "excluded_term": term})

    # The allergen check is primarily guaranteed by pre-filtering.
    # We preserve this output field for frontend validation compatibility.
    return {"allergen_violations": allergen_violations, "exclusion_violations": exclusion_violations, "diet_violations": diet_violations, "allergies_checked": normal_allergies,}

# ROUTES

@app.route("/")
def index():
    return render_template("index.html")


@app.route("/health", methods=["GET"],)
def health():
    return jsonify({"status": "ok", "backend_version": "usda-meal-lp-v7.11-final-recipes", "food_rows": int(len(foods)), "dataset": "USDA FoodData Central",})


@app.route("/optimize", methods=["POST"],)
def optimize_diet():

    try:
        data = (request.get_json(silent=True) or {})

        # INPUTS

        age = parse_nonnegative_int(data.get("age"), "Age",)

        children = parse_nonnegative_int(data.get("children"), "Number of children",)

        child_age = parse_nonnegative_int(data.get("child_age_months"), "Baby's age in months",)

        weight = parse_positive_float(data.get("weight"), "Weight",)

        height = parse_positive_float(data.get("height"), "Height",)

        country = str(data.get("country", "",)).strip()

        pregnant = normalize_text(data.get("pregnant", "no",))

        breastfeeding = normalize_text(data.get("breastfeeding", "",))

        diets = as_list(data.get("diet", [],))

        allergies = as_list(data.get("allergy", [],))

        cuisines = as_list(data.get("cuisine", [],))

        excluded_foods = as_list(data.get("excludedFoods", [],))

        if age < 18:
            return jsonify({"error": ("This recommender is currently configured " "for adult lactating women aged 18 years or older.")}), 400

        if breastfeeding == "no":
            return jsonify({"error": ("Recommendations are only generated " "for breastfeeding mothers.")}), 400

        if country not in COUNTRIES:
            return jsonify({"error": ("Please select a supported country.")}), 400

        # LACTATION STAGE

        if child_age <= 6:
            lactation_stage = ("Early lactation")

        elif child_age <= 12:
            lactation_stage = ("Mid lactation")

        else:
            lactation_stage = ("Late lactation")

        # GAM TARGETS

        protein_input = prepare_gam_input(age, children, weight, height, pregnant, child_age, lactation_stage, country, protein_model,)

        energy_input = prepare_gam_input(age, children, weight, height, pregnant, child_age, lactation_stage, country, energy_model,)

        protein_req = float(protein_model.predict(protein_input)[0])

        energy_req = float(energy_model.predict(energy_input)[0])

        if (not math.isfinite(protein_req) or protein_req <= 0):
            raise ValueError("The protein GAM returned an invalid target.")

        if (not math.isfinite(energy_req) or energy_req <= 0):
            raise ValueError("The energy GAM returned an invalid target.")

        # FILTER FOOD CATALOGUE

        filtered, filter_warnings = apply_filters(foods, diets, allergies, cuisines, excluded_foods,)

        if filtered.empty:
            return jsonify({"error": ("No foods remain after applying your " "diet, allergy, cuisine and food-exclusion choices."), "filter_warnings": filter_warnings,}), 400

        # BUILD MEAL POOLS

        (pools, sides, pool_notes,) = build_candidate_pools(filtered, cuisines,)

        required_roles = ["breakfast", "lunch", "snack", "supper",]

        empty_required = [role for role in required_roles if pools[role].empty]

        if empty_required:
            return jsonify({
                "error": (
                    "A safe complete daily meal plan could not be "
                    "constructed after applying the selected filters."
                ),
                "validation_errors": [
                    (
                        f"No eligible {role} foods remain."
                    )
                    for role in empty_required
                ],
                "filter_warnings": (
                    filter_warnings
                    + pool_notes
                ),
            }), 400

        # Pool candidate union for micronutrient-support checks.
        pool_frames = [frame for frame in pools.values() if not frame.empty]

        if not sides.empty:
            pool_frames.append(sides)

        candidate_union = (pd.concat(pool_frames, ignore_index=True,) .drop_duplicates(subset=["catalogue_id",]))

        # MICRONUTRIENT TARGETS

        all_targets = build_micronutrient_targets(age, energy_req,)

        (active_targets, nutrient_notes,) = determine_active_targets(candidate_union, all_targets, diets,)

        # SOLVE LP
        # If a cuisine is selected, first try a stricter solve that
        # uses curated recipes for breakfast/lunch/supper wherever a
        # curated option exists. If that is infeasible, retry with the
        # full cuisine-filtered pools so USDA dishes remain a fallback.
        curated_priority_applied_roles = []
        curated_priority_fallback_used = False

        solve_pools = pools

        if cuisines:
            (curated_priority_pools, curated_priority_applied_roles,) = build_curated_main_priority_pools(pools)

            if curated_priority_applied_roles:
                solve_pools = curated_priority_pools

        solution, solver_status = solve_meal_lp(solve_pools, sides, energy_req, protein_req, active_targets, enforce_variety=True,)

        if solution is None and cuisines and curated_priority_applied_roles:
            curated_priority_fallback_used = True
            solution, solver_status = solve_meal_lp(pools, sides, energy_req, protein_req, active_targets, enforce_variety=True)

        # Controlled fallback: relax only presentation-variety rules.
        # All nutrition, diet/allergy/exclusion safety, meal structure and portion rules stay hard.
        variety_fallback_used = False
        if solution is None:
            solution, solver_status = solve_meal_lp(pools, sides, energy_req, protein_req, active_targets, enforce_variety=False)
            if solution is not None:
                variety_fallback_used = True
                filter_warnings.append("Strict daily presentation-variety rules were relaxed because they made the current filtered food pool infeasible. Energy, protein, active hard micronutrient targets, diet/allergy/excluded-food safety, meal structure and portion limits remained enforced.")

        # Targeted B12 feasibility fallback for restrictive vegetarian plans.
        # B12 remains hard by default. Only after every normal solve has failed do
        # we test whether removing B12 alone restores feasibility. No target value
        # is lowered and all other nutrition/safety/meal/portion constraints remain.
        b12_feasibility_fallback_used = False
        normalized_diets_for_fallback = set(normalized_list(diets))
        b12_key = "vitamin-b12_100g"

        if (solution is None and "vegetarian" in normalized_diets_for_fallback and "vegan" not in normalized_diets_for_fallback and b12_key in active_targets):
            b12_relaxed_targets = {key: value for key, value in active_targets.items() if key != b12_key}

            # First preserve the normal hard presentation-variety rules.
            solution, solver_status = solve_meal_lp(pools, sides, energy_req, protein_req, b12_relaxed_targets, enforce_variety=True,)

            # If necessary, use the already-established cosmetic variety fallback.
            if solution is None:
                solution, solver_status = solve_meal_lp(pools, sides, energy_req, protein_req, b12_relaxed_targets, enforce_variety=False,)
                if solution is not None:
                    variety_fallback_used = True

            if solution is not None:
                active_targets = b12_relaxed_targets
                b12_feasibility_fallback_used = True
                nutrient_notes.append(
                    "Vitamin B12 normally remains a hard planning target for vegetarian plans. "
                    "For this combination of diet and food restrictions, the optimiser could not "
                    "produce a feasible plan while enforcing the 3.8 µg/day B12 minimum. Removing "
                    "Vitamin B12 alone restored feasibility, so the target is still displayed and "
                    "reported for additional attention rather than being lowered or silently ignored."
                )
                filter_warnings.append(
                    "A targeted Vitamin B12 feasibility fallback was used. All energy, protein, "
                    "other active hard micronutrient targets, diet/allergy/excluded-food safety, "
                    "meal structure and portion limits remained enforced."
                )
                if variety_fallback_used:
                    filter_warnings.append(
                        "Strict daily presentation-variety rules also had to be relaxed after the "
                        "Vitamin B12 feasibility fallback. Core nutrition, safety, meal-structure "
                        "and portion constraints remained enforced."
                    )

        if solution is None:
            diagnostic_notes = diagnose_infeasible_plan(pools, sides, energy_req, protein_req, active_targets,)

            return jsonify({
                "error": (
                    "No feasible meal plan could satisfy the "
                    "personalised energy target, the required "
                    "protein range (95–105% of the GAM target), "
                    "the active reliable vitamin/mineral planning "
                    "targets, the meal structure, the selected "
                    "food filters and the current portion limits."
                ),
                "solver_status": solver_status,
                "filter_warnings": filter_warnings,
                "validation_errors": (
                    pool_notes
                    + nutrient_notes
                    + diagnostic_notes
                ),
                "diagnostics": diagnostic_notes,
            }), 422

        # CALCULATE ACTUAL NUTRIENTS

        actual = calculate_actual_nutrients(solution, list(all_targets.keys()),)

        # BUILD MEAL OUTPUT

        recommendations, flat = build_meal_plan(solution)

        # FINAL VALIDATION

        energy_minimum = (energy_req * ENERGY_MIN_RATIO)

        energy_maximum = (energy_req * ENERGY_MAX_RATIO)

        protein_minimum = (protein_req * PROTEIN_MIN_RATIO)

        protein_maximum = (protein_req * PROTEIN_MAX_RATIO)

        energy_met = (actual["energy_kcal"] >= energy_minimum - ENERGY_VALIDATION_TOLERANCE_KCAL and actual["energy_kcal"] <= energy_maximum + ENERGY_VALIDATION_TOLERANCE_KCAL)

        protein_met = (actual["protein_g"] >= protein_minimum - PROTEIN_VALIDATION_TOLERANCE_G and actual["protein_g"] <= protein_maximum + PROTEIN_VALIDATION_TOLERANCE_G)

        micronutrient_met = {}

        for column, config in (active_targets.items()):
            micronutrient_met[config["label"]] = (actual.get(column, 0.0,) >= float(config["value"]) * 0.99)

        safety = final_safety_check(flat, allergies, excluded_foods, diets,)

        accepted = (
            energy_met
            and protein_met
            and all(
                micronutrient_met.values()
            )
            and not safety[
                "allergen_violations"
            ]
            and not safety[
                "exclusion_violations"
            ]
            and not safety[
                "diet_violations"
            ]
        )

        if not accepted:
            return jsonify({
                "error": (
                    "The final meal plan did not pass all "
                    "active nutrition and food-safety checks."
                ),
                "validation": {
                    "energy_target_met": energy_met,
                    "energy_actual_kcal": round(
                        actual[
                            "energy_kcal"
                        ],
                        4,
                    ),
                    "energy_minimum_kcal": round(
                        energy_minimum,
                        4,
                    ),
                    "energy_maximum_kcal": round(
                        energy_maximum,
                        4,
                    ),
                    "protein_target_met": protein_met,
                    "protein_actual_g": round(
                        actual[
                            "protein_g"
                        ],
                        4,
                    ),
                    "protein_minimum_g": round(
                        protein_minimum,
                        4,
                    ),
                    "protein_maximum_g": round(
                        protein_maximum,
                        4,
                    ),
                    "micronutrient_targets_met": (
                        micronutrient_met
                    ),
                },
                "safety_check": safety,
            }), 500

        # REPORT

        nutrient_report = build_nutrient_report(actual, energy_req, protein_req, all_targets, active_targets,)

        additional_attention = build_additional_attention(actual, all_targets, diets, active_targets,)

        warnings = list(filter_warnings)

        warnings.extend(nutrient_notes)

        if additional_attention:
            labels = ", ".join(item["label"] for item in additional_attention)

            warnings.append(
                "The following nutrients require additional attention "
                f"for the selected diet: {labels}. See the report guidance "
                "on fortified foods and discussing supplementation with a "
                "qualified healthcare professional."
            )

        warnings.append(
            "Iodine adequacy was not assessed because "
            "sufficiently complete iodine composition data "
            "were unavailable in the USDA catalogue used "
            "for optimisation."
        )

        if allergies:
            warnings.append("Selected allergies were filtered conservatively: " "foods marked 'unknown' for the selected allergen " "were not allowed.")

        if cuisines:
            warnings.append(
                "Cuisine preference was applied to prepared dishes, "
                "including the curated recipe catalogue. General USDA "
                "ingredients such as fruit, vegetables, grains and dairy "
                "remained available as supporting meal components."
            )

            if curated_priority_applied_roles:
                if curated_priority_fallback_used:
                    warnings.append(
                        "Curated cuisine recipes were tried first for "
                        + ", ".join(
                            curated_priority_applied_roles
                        )
                        + ", but the stricter curated-only main-meal solve "
                        "was infeasible, so USDA cuisine-matched prepared "
                        "dishes were allowed as a fallback."
                    )
                else:
                    warnings.append(
                        "Curated cuisine recipes were successfully prioritised "
                        "for "
                        + ", ".join(
                            curated_priority_applied_roles
                        )
                        + "; USDA prepared dishes remained available only as "
                        "a fallback."
                    )

        # RESPONSE

        return jsonify({
            "success": True,

            "backend_version": (
                "usda-meal-lp-v7.11-final-recipes"
            ),

            "personal_info": {
                "age": age,
                "country": country,
                "children": children,
                "weight": weight,
                "height": height,
                "pregnant": pregnant,
                "breastfeeding": breastfeeding,
                "child_age_months": child_age,
                "lactation_stage": lactation_stage,
            },

            "lactation_stage": (
                lactation_stage
            ),

            "gam_targets": {
                "energy_kcal": round(
                    energy_req,
                    1,
                ),
                "protein_g": round(
                    protein_req,
                    1,
                ),
                "energy_allowed_range_kcal": {
                    "minimum": round(
                        energy_req
                        * ENERGY_MIN_RATIO,
                        1,
                    ),
                    "maximum": round(
                        energy_req
                        * ENERGY_MAX_RATIO,
                        1,
                    ),
                },
                "protein_allowed_range_g": {
                    "minimum": round(
                        protein_req
                        * PROTEIN_MIN_RATIO,
                        1,
                    ),
                    "maximum": round(
                        protein_req
                        * PROTEIN_MAX_RATIO,
                        1,
                    ),
                },
            },

            "actual_nutrients": {
                "energy_kcal": round(
                    actual[
                        "energy_kcal"
                    ],
                    1,
                ),
                "protein_g": round(
                    actual[
                        "protein_g"
                    ],
                    1,
                ),
                **{
                    column: round(
                        value,
                        2,
                    )
                    for column, value
                    in actual.items()
                    if column not in {
                        "energy_kcal",
                        "protein_g",
                    }
                },
            },

            # Existing script.js can render these groups directly.
            "recommendations": (
                recommendations
            ),

            "meal_plan": (
                recommendations
            ),

            "recommendations_flat": (
                flat
            ),

            "nutrient_report": (
                nutrient_report
            ),

            "additional_attention": (
                additional_attention
            ),

            "filters_applied": {
                "diet": diets,
                "allergies": allergies,
                "cuisine": cuisines,
                "excluded_foods": excluded_foods,
            },

            "validation": {
                "validated": True,
                "recommendation_accepted": True,
                "energy_target_met": energy_met,
                "protein_target_met": protein_met,
                "protein_within_95_105_percent": protein_met,
                "micronutrient_targets_met": micronutrient_met,
                "allergies_removed": (
                    not safety[
                        "allergen_violations"
                    ]
                ),
                "excluded_foods_removed": (
                    not safety[
                        "exclusion_violations"
                    ]
                ),
                "diet_safety_passed": (
                    not safety[
                        "diet_violations"
                    ]
                ),
            },

            "solver": {
                "status": solver_status,
                "energy_range": (
                    "98–102% of GAM target"
                ),
                "protein_range": (
                    "95–105% of GAM target"
                ),
                "meal_structure": {
                    "breakfast": 1,
                    "lunch": 1,
                    "snack": 1,
                    "supper": 1,
                    "dessert": "0 or 1",
                    "supporting_items": (
                        f"2–{MAX_SUPPORTING_ITEMS}"
                    ),
                },
                "curated_cuisine_priority": {
                    "roles_attempted": curated_priority_applied_roles,
                    "fallback_to_usda_used": curated_priority_fallback_used,
                    "variety_fallback_used": variety_fallback_used,
                "b12_feasibility_fallback_used": b12_feasibility_fallback_used,
                },
            },

            "explanation": {
                "lactation_stage": (
                    lactation_stage
                ),
                "dataset": (
                    "USDA FoodData Central "
                    "(Foundation Foods + FNDDS) + curated USDA-linked recipes"
                ),
                "method": (
                    "Meal-based mixed-integer linear programming with "
                    "GAM energy/protein targets, USDA-linked curated recipes, "
                    "cuisine-aware prepared-dish filtering, ingredient-aware "
                    "food exclusions, semantic meal eligibility rules, daily "
                    "variety constraints, practical portion bounds and "
                    "food-quality penalties, curated-main cuisine priority and fruit-quality ranking"
                ),
            },

            "data_quality": {
                "dataset": (
                    "USDA FoodData Central + curated USDA-linked recipes"
                ),
                "usda_catalogue_rows": USDA_SOURCE_ROW_COUNT,
                "curated_recipe_rows": RECIPE_SOURCE_ROW_COUNT,
                "rows_before_filtering": int(
                    len(
                        foods
                    )
                ),
                "rows_after_filtering": int(
                    len(
                        filtered
                    )
                ),
                "shortlisted_optimization_candidates": int(
                    len(
                        candidate_union
                    )
                ),
                # Backward-compatible alias for older frontend code.
                "lp_candidate_foods": int(
                    len(
                        candidate_union
                    )
                ),
                "selected_foods": int(
                    len(
                        flat
                    )
                ),
                "meal_candidate_counts": {
                    role: int(
                        len(
                            frame
                        )
                    )
                    for role, frame
                    in pools.items()
                },
                "side_candidate_count": int(
                    len(
                        sides
                    )
                ),
                "notes": (
                    nutrient_notes
                    + [
                        (
                            "USDA nutrient values are used "
                            "directly in their final per-100-g units; "
                            "Open Food Facts unit conversion and "
                            "aggregation are no longer part of the "
                            "recommendation pipeline."
                        ),
                        (
                            "USDA household portion weights are "
                            "used where available. Practical "
                            "fallback gram portions are used only "
                            "when a suitable USDA serving weight "
                            "is unavailable."
                        ),
                        (
                            "The mandatory vegetable component is "
                            "restricted to whole or minimally processed "
                            "vegetable candidates; fries, tots, chips and "
                            "similar products do not satisfy that rule."
                        ),
                        (
                            "Sodium, fat and convenience-food indicators "
                            "are used as soft optimisation preferences so "
                            "that nutritionally feasible lower-penalty "
                            "plans are preferred."
                        ),
                        (
                            "Breakfast, lunch and supper mains are screened "
                            "for semantic meal suitability so sauces, syrups, "
                            "shells and similar components cannot represent "
                            "a complete main meal."
                        ),
                        (
                            "Supplement-like nutritional powders and meal "
                            "replacement products are excluded from the "
                            "recommendation catalogue at runtime."
                        ),
                        (
                            "Supporting foods are assigned to meal slots "
                            "using meal-aware rules so breakfast cereals and "
                            "similar foods are not attached to supper."
                        ),
                        (
                            "Supporting-food portion limits are adjusted by "
                            "food subtype; for example, dried fruit and nuts "
                            "have much smaller practical gram limits than "
                            "fresh fruit or cooked vegetables."
                        ),
                        (
                            "Dry milk powders and similar concentrated "
                            "ingredient powders are excluded from standalone "
                            "recommendation use. Ordinary milk and yogurt may "
                            "receive a candidate-ranking preference only when "
                            "compatible with the user's selected diet, "
                            "allergies and food exclusions."
                        ),
                        (
                            "Daily variety rules limit processed cereal, "
                            "granola or nutrition bars to at most one item "
                            "and prevent lunch and supper from using the "
                            "same recognisable main-dish family."
                        ),
                        (
                            "Food records labelled 'as ingredient' are "
                            "avoided as visible standalone sides when enough "
                            "directly servable alternatives are available."
                        ),
                        (
                            "Dried-fruit supporting portions are limited to "
                            "15–40 g to prevent concentrated fruit from being "
                            "selected in unrealistically large quantities."
                        ),
                        (
                            "Curated recipe serving sizes are reported using "
                            "the defined recipe serving rather than as a "
                            "generic fallback portion."
                        ),
                        (
                            "When a cuisine is selected, curated recipes receive "
                            "an additional ranking preference while USDA prepared "
                            "dishes remain available as feasibility fallbacks."
                        ),
                        (
                            "Standalone cereal supporting foods are limited to "
                            "20–60 g to prevent oversized cereal portions."
                        ),
                        (
                            "At most one standalone cereal supporting food is "
                            "allowed per day; an oat- or cereal-based breakfast "
                            "main may still be selected."
                        ),
                        (
                            "When a cuisine is selected, curated breakfast, "
                            "lunch and supper recipes are tried first. USDA "
                            "cuisine-matched prepared dishes are used only if "
                            "the stricter curated-main solve is infeasible."
                        ),
                        (
                            "Common standalone fruits such as banana, apple, "
                            "orange, mango, guava, berries and pineapple receive "
                            "a ranking preference, while unusual ingredient-like "
                            "fruit choices are softly discouraged."
                        ),
                        (
                            "Dessert remains optional and carries a soft selection "
                            "penalty so it is omitted when targets can be met without it."
                        ),
                        (
                            "Nuts and seeds use a practical 15 g reporting comparison "
                            "when a USDA cup measure is misleading; nutrient calculations "
                            "still use selected grams."
                        ),
                        (
                            "Very small USDA vegetable household measures, "
                            "such as a single leaf, are replaced only for "
                            "reporting with a practical 75 g comparison so "
                            "the displayed serving estimate remains meaningful."
                        ),
                        (
                            "Curated recipe nutrient composition is calculated "
                            "from linked USDA ingredient records; cuisine, diet "
                            "and allergen classifications are explicitly curated."
                        ),
                        (
                            "Excluded-food filtering checks curated recipe "
                            "ingredient names and ingredient keys in addition to "
                            "the displayed recipe name and description."
                        ),
                        (
                            "Nutritional drinks/shakes are excluded from ordinary "
                            "recommendation use, processed snack sides receive an "
                            "additional soft penalty, and misleading USDA household "
                            "descriptions for berries, muffins and small snack items "
                            "are replaced with practical reporting measures."
                        ),
                    ]
                ),
            },

            "dataset_information": {
                "name": (
                    "USDA FoodData Central + curated USDA-linked recipes"
                ),
                "lp_file": (
                    "usda_lp_food_catalogue.csv"
                ),
                "recipe_file": (
                    "recipe_catalogue_final.csv"
                ),
                "usda_source_rows": USDA_SOURCE_ROW_COUNT,
                "curated_recipe_rows": RECIPE_SOURCE_ROW_COUNT,
                "candidate_rows": int(
                    len(
                        foods
                    )
                ),
                "iodine_assessment": (
                    "not_available_for_current_LP"
                ),
            },

            "filter_warnings": (
                filter_warnings
            ),

            "warnings": (
                warnings
            ),

            "safety_check": (
                safety
            ),
        })

    except ValueError as exc:
        return jsonify({"error": str(exc)}), 400

    except Exception as exc:
        app.logger.exception("Unexpected optimization error")

        return jsonify({"error": ("An unexpected server error occurred " "while creating the meal plan."), "details": str(exc),}), 500

# RUN

if __name__ == "__main__":
    app.run(debug=True)