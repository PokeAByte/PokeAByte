import "material-icons/iconfont/filled.css";
import "./css/app.css"
import { Header } from "./layout/Header.tsx"
import { Footer } from "./layout/Footer.tsx"
import { MainView } from "./layout/MainView.tsx"
import { Notifications } from "./notifications/Notifications"
import { render } from "preact";
import { initializeRouting } from "./components/Route.tsx";

// @ts-expect-error "__POKEABYTE_VERSION__" is injected via vite.
document.title = "Poke-A-Byte " + __POKEABYTE_VERSION__;

initializeRouting("/ui");

render(
	<>
		<Header />
		<MainView />
		<Footer />
		<Notifications />
	</>,
	document.getElementById("root")!
)
