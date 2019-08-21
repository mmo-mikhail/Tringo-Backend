[![Build status](https://dev.azure.com/Tringo/Tringo/_apis/build/status/Tringo-CI-backend)](https://dev.azure.com/Tringo/Tringo/_build/latest?definitionId=7)

# Tringo-Backend
 Tringo - Back-End Application
 
 
 #### To Run on Windows in Docker (we use Linux containter):
 
 In root folder (which contains .sln file) run 2 commands:
 
 ###### 1. Build the image (tringoback - image name in this example; . - search Dockerfile in current folder):
 ```
 docker build -t tringoback .
```

###### 2. Run the image specifying port (5000 in this example):
```
docker run -it --rm -p 5000:80 --name tringoapp tringoback
```
