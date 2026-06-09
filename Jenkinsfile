pipeline {
    agent any

    stages {
        stage('Checkout SCM') {
            steps {
                echo 'Checking out code from SCM...'
                checkout scm
            }
        }

        stage('Checkout') {
            steps {
                echo 'Code checkout complete.'
            }
        }

        stage('Build') {
            steps {
                echo 'Building backend application...'
                sh 'dotnet build -c Release'
            }
        }

        stage('Build Docker') {
            steps {
                echo 'Building Docker image...'
                sh 'docker build -t todo-backend:latest .'
            }
        }

        stage('Start MySQL') {
            steps {
                echo 'Starting MySQL container...'
                sh '''
                    docker network create todo-network || true
                    docker stop todo-mysql || true
                    docker rm todo-mysql || true
                    docker run -d --name todo-mysql \
                      --network todo-network \
                      -p 3307:3306 \
                      -e MYSQL_ROOT_PASSWORD=todopass \
                      -e MYSQL_DATABASE=TodoDB \
                      -v mysql_data:/var/lib/mysql \
                      --health-cmd="mysqladmin ping -h localhost" \
                      --health-interval=5s \
                      --health-timeout=5s \
                      --health-retries=5 \
                      mysql:8.0
                '''
            }
        }

        stage('MySQL Health Check') {
            steps {
                echo 'Waiting for MySQL to be healthy...'
                sh '''
                    for i in {1..30}; do
                        status=$(docker inspect --format='{{.State.Health.Status}}' todo-mysql 2>/dev/null || echo "failed")
                        echo "Current MySQL status: $status"
                        if [ "$status" = "healthy" ]; then
                            echo "MySQL is healthy!"
                            exit 0
                        fi
                        sleep 5
                    done
                    echo "MySQL failed to become healthy in time."
                    exit 1
                '''
            }
        }

        stage('Run API') {
            steps {
                echo 'Starting backend API container...'
                sh '''
                    docker stop todo-backend-container || true
                    docker rm todo-backend-container || true
                    docker run -d --name todo-backend-container \
                      --network todo-network \
                      -p 5143:5143 \
                      -e ConnectionStrings__DefaultConnection="Server=todo-mysql;Database=TodoDB;User=root;Password=todopass;" \
                      -e Jwt__Key="your-super-secret-key-that-should-be-at-least-32-characters-long-for-hs256" \
                      -e Jwt__Issuer="Todo.Backend" \
                      -e Jwt__Audience="Todo.Frontend" \
                      todo-backend:latest
                '''
            }
        }
    }

    post {
        always {
            echo 'Pipeline execution completed.'
        }
        success {
            echo 'Backend build and deployment successful!'
        }
        failure {
            echo 'Backend build or deployment failed!'
        }
    }
}
