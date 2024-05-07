# from the dropdown at the top of Cloud Console:
export GCLOUD_PROJECT="ytm-api-mp3" 
# from Step 2.2 above:
export REPO="youtube-dl-api-repo"
# the region you chose in Step 2.4:
export REGION="europe-west2"
# whatever you want to call this image:
export IMAGE="youtube-dl-api-image"

# use the region you chose above here in the URL:
export IMAGE_TAG=${REGION}-docker.pkg.dev/$GCLOUD_PROJECT/$REPO/$IMAGE

echo ${IMAGE_TAG}

# Build the image:
docker build -t $IMAGE_TAG .
# Push it to Artifact Registry:
docker push $IMAGE_TAG