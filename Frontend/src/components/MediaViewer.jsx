export default function MediaViewer({ imageUrl, audioUrl }) {
  return (
    <div className="mt-6 p-4 bg-gray-50 rounded-lg border border-gray-200">
      <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wider mb-4">Attached Files</h3>
      
      <div className="flex flex-col gap-6">
        {/* Image Viewer */}
        {imageUrl ? (
          <div>
             <span className="text-xs text-gray-500 mb-2 block">Screenshot</span>
             <a href={imageUrl} target="_blank" rel="noopener noreferrer">
                <img 
                  src={imageUrl} 
                  alt="Ticket Attachment" 
                  className="max-w-full md:max-w-md h-auto rounded-lg shadow-sm border border-gray-200 hover:opacity-90 transition-opacity cursor-zoom-in"
                />
             </a>
          </div>
        ) : (
          <p className="text-sm text-gray-400 italic">No image attached.</p>
        )}

        {/* Audio Player */}
        {audioUrl && (
          <div>
            <span className="text-xs text-gray-500 mb-2 block">Voice Note</span>
            <audio controls className="w-full md:max-w-md" src={audioUrl}>
              Your browser does not support the audio element.
            </audio>
          </div>
        )}
      </div>
    </div>
  );
}