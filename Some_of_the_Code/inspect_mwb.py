import zipfile
import sys
import xml.etree.ElementTree as ET

def get_value(obj, key):
    for child in obj.findall("value"):
        if child.get("key") == key:
            return (child.text or "").strip()
    return ""

with zipfile.ZipFile(sys.argv[1]) as archive:
    root = ET.fromstring(archive.read("document.mwb.xml"))

for table in root.iter("value"):
    if table.get("struct-name") != "db.mysql.Table":
        continue

    print("\nTABLE:", get_value(table, "name"))

    for child in table.findall("value"):
        if child.get("key") != "columns":
            continue

        for column in child.findall("value"):
            if column.get("struct-name") != "db.mysql.Column":
                continue

            name = get_value(column, "name")
            auto = get_value(column, "autoIncrement") == "1"
            not_null = get_value(column, "isNotNull") == "1"

            datatype = "unknown"
            for link in column.findall("link"):
                if link.get("key") == "simpleType":
                    datatype = (link.text or "").split(".")[-1]

            print(f"  {name}: {datatype}, NOT NULL={not_null}, AUTO_INCREMENT={auto}")
