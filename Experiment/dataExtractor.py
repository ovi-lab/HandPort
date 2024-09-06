import csv
import json

# Function to load JSON data from a specified file path
def load_json_from_file(file_path):
    with open(file_path, encoding='utf-8') as json_file:
        data = json.load(json_file)
    return data

# Function to write the log data to a CSV file
def write_log_to_csv(data, output_file):
    with open(output_file, mode='w', newline='') as file:
        writer = csv.writer(file)

        # Write the header row if needed (Optional)
        writer.writerow(['log_data'])  # Modify this based on the format you prefer
        # Iterate over the log entries and write to the CSV
        for entry in data['log']:
            for key, value in entry.items():
                value.rstrip()
                
                # Write the extracted value to the CSV
                writer.writerow([value.strip(", ")])

    print(f"Data has been written to {output_file}")

# Main function to handle the process
def main():
    # Specify the path to the JSON file
    json_file_path = "C:/Users/obaheti/Repos/GoGoTeleportation/Experiment/go-go-teleportation-default-rtdb-export.json"

    # Load the JSON data from the file
    data = load_json_from_file(json_file_path)

    if data is None:
        print("Failed to load JSON data. Exiting...")
        return

    # Specify the output CSV file path
    output_file_path = "C:/Users/obaheti/Repos/GoGoTeleportation/Experiment/output.csv"

    # Write the log data to the CSV
    write_log_to_csv(data, output_file_path)

if __name__ == "__main__":
    main()