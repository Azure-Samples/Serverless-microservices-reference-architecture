import json
import logging
import pickle
import os
import numpy as np
import pandas as pd
from sklearn.externals import joblib
from azureml.core.model import Model
from azureml.train import automl

from inference_schema.schema_decorators import input_schema, output_schema
from inference_schema.parameter_types.numpy_parameter_type import NumpyParameterType
from inference_schema.parameter_types.pandas_parameter_type import PandasParameterType

scriptpath = os.path.abspath(__file__)
scriptdir = os.path.dirname(scriptpath)
filename = os.path.join(scriptdir, 'model.pkl')

input_sample = pd.DataFrame(data=[{"Date":"2013-01-01T00:00:00.000Z","Battery_ID":0,"Battery_Age_Days":0,"Daily_Trip_Duration":67.8456075842}])

def _initialize():
    global model
    # This name is model.id of model that we want to deploy deserialize the model file back
    # into a sklearn model
    
    #model = joblib.load(model_path)
    logging.info(filename)

    if os.path.getsize(filename) > 0:
        model = pickle.load(open(filename,'rb'))
    else:
        logging.info("Could not load the model. It is empty!")
    logging.info("Model loaded")

@input_schema('data', PandasParameterType(input_sample))
def predict_battery_failure(data):
    try:
        _initialize()
        
        #logging.info(data["Battery_Age_Days"][0]);

        result = model.predict(data)
    except Exception as e:
        result = str(e)
        return json.dumps({"error": result})

    #forecast_as_list = result[0].tolist()
    #index_as_df = result[1].index.to_frame().reset_index(drop=True)
    
    #return json.dumps({"forecast": forecast_as_list,   # return the minimum over the wire: 
    #                   "index": json.loads(index_as_df.to_json(orient='records'))  # no forecast and its featurized values
    #                  })
    return result.tolist()