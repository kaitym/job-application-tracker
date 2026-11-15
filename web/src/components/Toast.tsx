// A little pop-up message at the bottom of the screen
export function Toast({ message }: { message: string | null }) {
  return (
    <div aria-live="polite" className="pointer-events-none fixed inset-x-0 bottom-6 z-20 flex justify-center px-4">
      {message && (
        <p key={message} className="toast-in rounded-full bg-espresso px-5 py-3 text-sm font-medium text-whitewash shadow-lg">
          {message}
        </p>
      )}
    </div>
  )
}
