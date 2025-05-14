import "material-icons/iconfont/filled.css";
import "./css/app.css"
import { Router } from "wouter"
import { Header } from "./layout/Header.tsx"
import { Footer } from "./layout/Footer.tsx"
import { MainView } from "./layout/MainView.tsx"
import { Notifications } from "./notifications/Notifications"
import { render } from "preact";
import { AdvancedFeatureContextProvider } from "./Contexts/advancedFeatureContext.tsx";

render(
	<Router base="/ui">
		<AdvancedFeatureContextProvider>
			<Header />
			<MainView />
			<Footer />
			<Notifications />
		</AdvancedFeatureContextProvider>
	</Router>,
	document.getElementById("root")!
)
