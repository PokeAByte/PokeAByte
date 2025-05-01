
export function LoadProgress({ label }: { label?: string }) {
  return (
    <>
      {!!label &&
        <h2>
          {label}
        </h2>
      }
      <div>
        <progress className="error" />
      </div>
    </>
  );
}