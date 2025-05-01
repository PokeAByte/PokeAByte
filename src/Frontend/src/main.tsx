import { createRoot } from "react-dom/client"
import App from "./App.tsx"
import { Router } from "wouter"

createRoot(document.getElementById("root")!).render(
	<Router base="/ui">
		<App />
	</Router>
)
