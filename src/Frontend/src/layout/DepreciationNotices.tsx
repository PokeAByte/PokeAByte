import { useAPI } from "@/hooks/useAPI";
import { useStorageState } from "@/hooks/useStorageState";
import { getDriverName } from "@/utility/fetch";
import { ComponentChildren } from "preact";
import { useEffect, useRef, useState } from "preact/hooks";

export function DepreciationNotices(props: { mapperId: string | null; }) {
	const driverApi = useAPI(getDriverName);
	useEffect(() => {
		driverApi.call();
	}, [props.mapperId]);
	if (driverApi.result === "RetroArch") {
		return <RetroarchWarning />;
	}
	if (driverApi.result === "Bizhawk") {
		return <BizhawkWarning />;
	}
	return null;
}

export function RetroarchWarning() {
	return (
		<Warning flag="_retroArchDepreciation">
			<h2>Notice:</h2>
			<p >
				The driver for RetroArch and SuperShuckie (1.0) is deprecated. 
				<br/>Support for it will be removed in the future.
				<br/>
				Consider switching to another
				{" "}<a target="_blank" href="https://github.com/PokeAByte/PokeAByte/blob/main/docs/HowTo.md">
					compatible emulator
				</a>.
			</p>
		</Warning>
	)
}

export function BizhawkWarning() {
	return (
		<Warning flag="_bizhawkDepreciation">
			<h2>Notice:</h2>
			<p >
				The external tools "Poke-A-Byte Integration" and "Poke-A-Byte STP Tool" are deprecated. <br/>
				If you wish to continue using BizHawk as your emulator, please switch to the "Emulator Data Protocol Server".
				<br/>
				<a target="_blank" href="https://github.com/PokeAByte/PokeAByte/blob/main/docs/HowTo.md">
					Information as to how can be found here.
				</a>
				<br/>Support for these tools will be removed in the Future.
			</p>
		</Warning>
	)
}

export function Warning(props: { flag: string, children: ComponentChildren}) {
	const [closed, setClosed] = useState(false)
	const dialogRef = useRef<HTMLDialogElement>(null);
	const [disabled, setDisabled] = useStorageState(props.flag, false);
	const onClose = () => {
		setClosed(true);
		sessionStorage.setItem(props.flag, "true");
	}
	useEffect(
		() => {
			if (dialogRef.current && !sessionStorage.getItem(props.flag)) {
				dialogRef.current.showModal();
			}
		},
		[dialogRef]
	);
	if (disabled || closed) {
		return null;
	}
	return (
		<dialog ref={dialogRef} onToggle={(e) => e.newState === "closed"}>
			{props.children} 
			<div>
				<button type="button" class="blue margin-right" onClick={() => setDisabled(true)}>
					Don't show again
				</button>
				<button type="button" class="red" onClick={onClose}>
					Close
				</button>
			</div>
		</dialog>
	)
}