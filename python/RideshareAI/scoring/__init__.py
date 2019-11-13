import logging
import datetime
import json
import pandas as pd
import azure.functions as func

# Import scoring script.
from .scoring_service import predict_battery_failure

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    req_body = req.get_json()
    data = req_body

    if data:
        batteryAgeDays = data["batteryAgeDays"]
        batteryRatedCycles = data["batteryRatedCycles"]
        lifetimeBatteryCyclesUsed = data["lifetimeBatteryCyclesUsed"]
        dailyTripDuration = data["dailyTripDuration"]
        #inputs = json.loads(data)

        now = datetime.datetime.today()
        inputs = [{"Date":now.ctime(),"Battery_ID":0,"Battery_Age_Days":batteryAgeDays,"Daily_Trip_Duration":dailyTripDuration}]
        logging.info(inputs)

        results = predict_battery_failure(inputs)

        headers = {
            "Content-type": "application/json",
            "Access-Control-Allow-Origin": "*"
        }

        # The results return in an array of doubles. We only expect a single result for this prediction.
        # The prediction returned represents the predicted battery cycles consumed within a given day.
        predictedDailyCyclesUsed = results[0]

        logging.info("Predicted daily cycles used: " + str(predictedDailyCyclesUsed))
        logging.info("Predicted daily cycles used in next 30 days: " + str(predictedDailyCyclesUsed * 30))
        logging.info("Predicted lifetime cycles used after 30 days: " + str(predictedDailyCyclesUsed * 30 + lifetimeBatteryCyclesUsed))
        
        # Multiply the predictedCyclesConsumed * 30 (days), add that value to the lifetime cycles used, then see if it exceeds the battery's rated cycles.
        predictedToFail = predictedDailyCyclesUsed * 30 + lifetimeBatteryCyclesUsed > batteryRatedCycles

        return func.HttpResponse(json.dumps(predictedToFail), headers = headers)
    else:
        return func.HttpResponse(
             "Please pass the battery data on the query string or in the request body",
             status_code=400
        )
