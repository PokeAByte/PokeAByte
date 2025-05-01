import { useLocation } from "wouter";
import { HeaderNavigation } from "./HeaderNavigation";
import { Mapper } from "pokeaclient";

export function Header(props: { mapper: Mapper | null }) {
	const [, setLocation] = useLocation();
	const onPowerButtonClick = () => {
		setLocation("");
	};

	const textHighlightClass = props.mapper ? "text-green" : "text-red";
	return (
		<header className="layout-box">
			<div>
				<h1>
					Poke-A-Byte
				</h1>
				<button
					type="button"
					onClick={onPowerButtonClick}
					title={props.mapper ? "Status: Connected" : "Status: Disconnected"}
				>
					<i className={`material-icons ${textHighlightClass}`}> power_settings_new </i>
				</button>
			</div>
			<nav className="tab">
				<HeaderNavigation mapper={props.mapper} />
			</nav>
		</header>
	);
}