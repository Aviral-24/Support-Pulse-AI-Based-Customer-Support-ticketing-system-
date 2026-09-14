import { InboxIcon } from 'lucide-react'; // Empty state icon

// 🦴 Skeleton Component (animate-pulse)
const TicketSkeleton = () => (
  <div className="p-4 bg-white rounded-lg shadow-sm border border-gray-100 flex gap-4 animate-pulse">
    <div className="w-12 h-12 bg-gray-200 rounded-full"></div>
    <div className="flex-1 space-y-3 py-1">
      <div className="h-4 bg-gray-200 rounded w-3/4"></div>
      <div className="h-3 bg-gray-200 rounded w-1/2"></div>
      <div className="h-3 bg-gray-200 rounded w-5/6"></div>
    </div>
  </div>
);

// 📭 Empty State Component
const EmptyState = () => (
  <div className="flex flex-col items-center justify-center p-12 text-center bg-white rounded-xl border border-dashed border-gray-300">
    <div className="w-16 h-16 bg-blue-50 rounded-full flex items-center justify-center mb-4">
      <InboxIcon className="w-8 h-8 text-blue-500" />
    </div>
    <h3 className="text-lg font-medium text-gray-900">No Tickets Found</h3>
    <p className="text-gray-500 mt-1">You haven't raised any tickets yet. Create one to get started!</p>
  </div>
);

export default function TicketList({ isLoading, tickets }) {
  // Loading State (Showing 3 skeletons)
  if (isLoading) {
    return (
      <div className="space-y-4 max-w-2xl mx-auto mt-8">
        <TicketSkeleton />
        <TicketSkeleton />
        <TicketSkeleton />
      </div>
    );
  }

  // Empty State
  if (!tickets || tickets.length === 0) {
    return (
      <div className="max-w-2xl mx-auto mt-8">
        <EmptyState />
      </div>
    );
  }

  // Data State (List rendering)
  return (
    <div className="space-y-4 max-w-2xl mx-auto mt-8">
      {tickets.map((ticket) => (
        <div key={ticket.id} className="p-4 bg-white rounded-lg shadow-sm border border-gray-100">
          <h4 className="font-semibold text-gray-800">{ticket.title}</h4>
          <p className="text-gray-600 text-sm mt-1">{ticket.description}</p>
        </div>
      ))}
    </div>
  );
}