
# VR-Library: Backend Infrastructure & Deployment Protocol

This document outlines the standard operating procedures for deploying the VR-Library backend environment.


## Option A: Containerized Quick Start (Highly Recommended)

*The fastest way to spin up the backend isolated from your local OS environments.*

### 1. Prerequisites (Windows / macOS)

- Install **Docker Desktop**: [Download Here](https://www.docker.com/products/docker-desktop/)
- Ensure Docker Desktop is running (the whale icon in your system tray must be green).
- **WSL2** must be enabled if you are using Windows.

### 2. Database Initialization

Before spinning up the cluster, you must initialize the local MySQL database.

1. Navigate to the database script directory:

    ```
    cd build/db
    ```

2. Execute the provided `.sql` files in your preferred local database GUI (e.g., Navicat, DataGrip) to generate the schemas. *(If using the full docker-compose, this mounts automatically to `/docker-entrypoint-initdb.d`)*.

### 3. Env Cluster Boot

1. Navigate to the `build` directory:

    ```
    cd build
    ```

2. Spin up the cluster in detached mode:

    ```
    docker-compose up -d
    ```
### 4. Container Image Building (For CI/CD & Releases)

*If you are responsible for packaging the final delivery artifacts:*

**Stage 1: Global Compilation**

```
mvn clean package -DskipTests
```

**Stage 2: Root-Level Docker Encapsulation**

```
mvn docker:build -N
```
*(The `-N` flag is mandatory to prevent Maven Reactor topological sorting errors across dependency modules).*

If these steps success, you will see these images below

```angular2html

(base) dodge@DodgedeMacBook-Air ~ % docker images
REPOSITORY           TAG       IMAGE ID       CREATED         SIZE
vr-library-backend   1.1.0     84ea996bd256   7 seconds ago   546MB
eclipse-temurin      17-jre    ff692acf8725   3 days ago      398MB

```

**Stage 3: Run backend Docker container**
```
docker run -d -p 6201:6201   -e SPRING_PROFILES_ACTIVE=test  --name vr-backend-standalone --network build_survey-network vr-library-backend:1.1.0
```


*The backend API is now accessible at `http://localhost:6201`. API documentation (Swagger/Knife4j) is available at `http://localhost:6201/doc.html`.*

## Option B: Native Source Compilation (Advanced)

*Use this option if you need to modify backend logic or prefer running the Java application directly on your host machine.*

### 1. Strict Environment Requirements

Failure to match these exact versions may result in compilation or runtime `ExceptionInInitializerError`.

- **JDK**: Version 17 (Eclipse Temurin or Oracle). Ensure `JAVA_HOME` is correctly set in your OS path.
- **Apache Maven**: Version 3.8.x or higher.
- **PostgreSQL Server**: Version 8.0+ running natively on your `localhost:5432`.
- **Redis Server**: Version 7.0+ running natively on your `localhost:6379`.
- **MinIO**: Running locally for object storage (optional depending on active profiles).

### 2. Local Configuration

You must manually configure your database connections before compiling.

1. Navigate to `library-boot/src/main/resources/`.
2. Modify `application-dev.yml` (or your active profile) to match your local native Env.

### 3. Compilation & Execution

1. Navigate to the project root directory.
2. Execute the Maven lifecycle to resolve dependencies and compile the multi-module project:

    ```
    mvn clean install -DskipTests
    ```

3. Boot the Spring application natively:

    ```
    cd library-boot
    mvn spring-boot:run
    ```
