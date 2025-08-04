import requests
import json

def test_ml_api():
    url = "http://localhost:5000/predict"
    
    # Test cases with different latency values
    test_cases = [
        {"latency_ms": 50},   # Normal latency
        {"latency_ms": 150},  # Normal latency
        {"latency_ms": 500},  # High latency
        {"latency_ms": 1000}, # Very high latency
        {"latency_ms": 2000}, # Extremely high latency
    ]
    
    print("Testing ML API...")
    print("=" * 50)
    
    for i, test_case in enumerate(test_cases, 1):
        try:
            response = requests.post(url, json=test_case, timeout=5)
            if response.status_code == 200:
                result = response.json()
                anomaly = result.get('anomaly', 'unknown')
                print(f"Test {i}: Latency {test_case['latency_ms']}ms -> Anomaly: {anomaly}")
            else:
                print(f"Test {i}: Error - Status {response.status_code}")
        except requests.exceptions.RequestException as e:
            print(f"Test {i}: Connection error - {e}")
    
    print("=" * 50)

if __name__ == "__main__":
    test_ml_api() 