#!/bin/bash
set -e

cd ../nodejs/serverless-microservices-functionapp-triparchiver
npm install
npm run pack
cd ../pipelines
