# Frontend 

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.1.6.

## Running the Frontend

Install dependencies:
```sh
npm install
```  

Start the development server:  
```sh
npm start
```

Open `http://localhost:4200/` in your browser.

## Backend connection

The app loads loans from the backend API. Make sure the API is running first
(see `../backend/src/README.md` — the easiest path is `docker compose up` from
the repository root).

The API base URL is configured in `src/environments/environment.ts`
(`apiUrl`, default `http://localhost:5000`). The backend allows the Angular dev
origin (`http://localhost:4200`) via CORS.
