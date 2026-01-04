import "material-icons/iconfont/filled.css";

import "./css/app.css"
import { Router } from "wouter"
import { Header } from "./layout/Header.tsx"
import { Footer } from "./layout/Footer.tsx"
import { MainView } from "./layout/MainView.tsx"
import { Notifications } from "./notifications/Notifications"
import { render } from "preact";
import { UISettingsProvider } from "./Contexts/UISettingsContext.tsx";
import { MapperFilesContextProvider } from "./Contexts/availableMapperContext.tsx";

render(
	<Router base="/ui">
		<UISettingsProvider>
			<MapperFilesContextProvider>
				<Header />
				<MainView />
				<Footer />
				<Notifications />
			</MapperFilesContextProvider>
		</UISettingsProvider>
	</Router>,
	document.getElementById("root")!
)
