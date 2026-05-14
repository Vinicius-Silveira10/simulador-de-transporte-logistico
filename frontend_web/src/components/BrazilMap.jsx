import { MapContainer, TileLayer, CircleMarker, Tooltip, Polyline, Marker } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';

// Base da Empresa (Sede HQ) será sempre São Paulo
const hqLocation = { name: "Sede Logística Central - São Paulo, SP", lat: -23.5505, lng: -46.6333 };

// Ícone do Radar Pulsante
const radarIcon = L.divIcon({
  className: 'radar-container',
  html: '<div class="radar-pulse"></div>',
  iconSize: [20, 20],
  iconAnchor: [10, 10]
});

// Lista de destinos logísticos espalhados pelo Brasil
const cityNodes = [
  { name: "São Paulo, SP", lat: -23.5505, lng: -46.6333 },
  { name: "Rio de Janeiro, RJ", lat: -22.9068, lng: -43.1729 },
  { name: "Belo Horizonte, MG", lat: -19.9208, lng: -43.9378 },
  { name: "Curitiba, PR", lat: -25.4284, lng: -49.2733 },
  { name: "Porto Alegre, RS", lat: -30.0346, lng: -51.2177 },
  { name: "Salvador, BA", lat: -12.9714, lng: -38.5014 },
  { name: "Recife, PE", lat: -8.0476, lng: -34.8770 },
  { name: "Fortaleza, CE", lat: -3.7172, lng: -38.5247 },
  { name: "Manaus, AM", lat: -3.1190, lng: -60.0217 },
  { name: "Brasília, DF", lat: -15.7975, lng: -47.8919 },
];

const getCityForCoordinate = (coordStr) => {
  if (!coordStr) return cityNodes[1]; // Fallback
  let hash = 0;
  for (let i = 0; i < coordStr.length; i++) {
    hash = coordStr.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % cityNodes.length;
  if(index === 0) return cityNodes[1]; 
  return cityNodes[index];
};

export default function BrazilMap({ trips }) {
  const tripMarkers = trips.map(trip => {
    const city = getCityForCoordinate(trip.destination);
    return { ...trip, ...city };
  });

  return (
    <div className="glass-card" style={{ height: "450px", padding: "10px", width: "100%", boxSizing: "border-box" }}>
      <h2 style={{marginTop: "5px", marginBottom: "15px", color: 'var(--text-primary)', marginLeft: "10px"}}>📍 Hub Logístico Nacional</h2>
      
      <MapContainer 
        center={[-14.2350, -51.9253]} 
        zoom={4} 
        style={{ height: "calc(100% - 40px)", width: "100%", borderRadius: "12px", background: "#0f172a", zIndex: 0 }}
        scrollWheelZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://carto.com/">Carto</a>'
          url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
        />
        
        <CircleMarker 
            center={[hqLocation.lat, hqLocation.lng]} 
            pathOptions={{ color: "var(--accent-red)", fillColor: "var(--accent-red)", fillOpacity: 0.9 }}
            radius={10}
        >
          <Tooltip direction="top" offset={[0, -10]} opacity={1} permanent>
             🏢 {hqLocation.name}
          </Tooltip>
        </CircleMarker>

        {tripMarkers.map(marker => (
          <div key={marker.id}>
            <Polyline 
              positions={[ [hqLocation.lat, hqLocation.lng], [marker.lat, marker.lng] ]} 
              pathOptions={{ 
                color: marker.status === "Finished" ? "var(--accent-green)" : "var(--accent-cyan)", 
                weight: marker.status === "Finished" ? 2 : 3,
                dashArray: marker.status === "Finished" ? null : "8, 8",
                lineCap: 'round',
                opacity: marker.status === "Finished" ? 0.3 : 0.8
              }}
            />
            {marker.status !== "Finished" ? (
              <Marker position={[marker.lat, marker.lng]} icon={radarIcon}>
                <Tooltip>
                  <strong>Frete em Trânsito</strong><br/>
                  Destino: {marker.name}
                </Tooltip>
              </Marker>
            ) : (
              <CircleMarker 
                center={[marker.lat, marker.lng]} 
                pathOptions={{ 
                  color: "var(--accent-green)", 
                  fillColor: "var(--accent-green)", 
                  fillOpacity: 0.8 
                }}
                radius={8}
              >
                <Tooltip>
                  Entregue: {marker.name}
                </Tooltip>
              </CircleMarker>
            )}
          </div>
        ))}
      </MapContainer>
    </div>
  );
}
