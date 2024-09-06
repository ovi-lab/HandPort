import csv

def delete_last_character_from_each_row(file_path):
    try:
        # Read the CSV file content
        with open(file_path, 'r') as file:
            reader = csv.reader(file)
            rows = [row for row in reader]
        
        # Modify each row by removing the last character of the last column
        updated_rows = []
        for row in rows:
            if row:  # Check if the row is not empty
                # Remove the last character of each element in the row
                updated_row = [element[:-1] if element else element for element in row]
                updated_rows.append(updated_row)
        
        # Write the updated content back to the CSV file
        with open(file_path, 'w', newline='') as file:
            writer = csv.writer(file)
            writer.writerows(updated_rows)
        
        print("Last character removed from each row successfully.")
    
    except FileNotFoundError:
        print(f"File not found: {file_path}")
    except Exception as e:
        print(f"An error occurred: {e}")

# Example usage
file_path = 'C:/Users/obaheti/Repos/GoGoTeleportation/Experiment/output.csv'  # Replace with the actual file path
delete_last_character_from_each_row(file_path)
