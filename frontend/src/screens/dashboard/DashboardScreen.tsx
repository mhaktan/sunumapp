import React from 'react';
import { TkCard } from '@takeoff-ui/react';

import {
  ResponsiveContainer,
  LineChart, Line,
  BarChart, Bar,
  PieChart, Pie, Cell,
  CartesianGrid, XAxis, YAxis, Tooltip,
} from 'recharts';

import { API_BASE } from '../../config';
import { getRequestHeaders } from '../../dataProvider';


const chartData = [
  { name: 'Jan', value: 400, value2: 240 },
  { name: 'Feb', value: 300, value2: 139 },
  { name: 'Mar', value: 600, value2: 380 },
  { name: 'Apr', value: 450, value2: 290 },
  { name: 'May', value: 700, value2: 480 },
  { name: 'Jun', value: 550, value2: 420 },
];

const LOADING_KEYFRAMES = `
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }
@keyframes spin { to { transform: rotate(360deg); } }
`;

export const DashboardScreen: React.FC = () => {
  const getNestedValue = (obj: Record<string, unknown>, path: string): unknown => {
    const direct = path.split('.').reduce<unknown>((o, k) => (o && typeof o === 'object') ? (o as Record<string, unknown>)[k] : undefined, obj);
    if (direct !== undefined) return direct;
    // Fallback: try last segment at top level (handles ABP-style {result: {...}} unwrapping)
    const segments = path.split('.');
    if (segments.length > 1 && obj && typeof obj === "object") {
      const lastKey = segments[segments.length - 1];
      const top = (obj as Record<string, unknown>)[lastKey];
      if (top !== undefined) return top;
    }
    return undefined;
  };

  // Unwrap common API envelopes: ABP {result, __abp}, generic {data}, etc.
  const unwrapResponse = (json: unknown): unknown => {
    if (!json || typeof json !== "object" || Array.isArray(json)) return json;
    const obj = json as Record<string, unknown>;
    // ABP envelope: {result, success, error, __abp}
    if ("__abp" in obj && "result" in obj) return obj.result;
    // Generic envelope: {success: true, data: X}
    if ("success" in obj && "data" in obj && Object.keys(obj).length <= 4) return obj.data;
    return json;
  };

  const extractArray = (json: unknown): Record<string, unknown>[] => {
    if (Array.isArray(json)) return json;
    if (json && typeof json === 'object') {
      const obj = json as Record<string, unknown>;
      for (const key of ['items', 'data', 'results', 'records', 'rows', 'list']) {
        if (Array.isArray(obj[key])) return obj[key] as Record<string, unknown>[];
      }
      // Recurse one level — handles {result: {items: [...]}}
      for (const val of Object.values(obj)) {
        if (val && typeof val === 'object' && !Array.isArray(val)) {
          const inner = val as Record<string, unknown>;
          for (const key of ['items', 'data', 'results', 'records', 'rows', 'list']) {
            if (Array.isArray(inner[key])) return inner[key] as Record<string, unknown>[];
          }
        }
        if (Array.isArray(val)) return val as Record<string, unknown>[];
      }
    }
    return [];
  };

  const [api_block_1Data, setApi_block_1Data] = React.useState<Record<string, unknown> | null>(null);
  const [api_block_1Loading, setApi_block_1Loading] = React.useState(true);
  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch(`${API_BASE}/api/services/app/Aircraft/GetAll`, { headers: getRequestHeaders() });
        // Token expired or invalid — clear auth and redirect to login
        if (res.status === 401) {
          ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k));
          if (window.location.pathname !== '/login') window.location.href = '/login';
          return;
        }
        if (!res.ok) return;
        const rawJson = await res.json();
        // Auto-unwrap ABP/generic envelopes so {result.totalCount} or bare {totalCount} both work
        const json = unwrapResponse(rawJson) as Record<string, unknown>;
        setApi_block_1Data(json);
      } catch { /* ignore */ }
      finally { setApi_block_1Loading(false); }
    };
    fetchData();
  }, []);

  const [api_block_2Data, setApi_block_2Data] = React.useState<Record<string, unknown> | null>(null);
  const [api_block_2Loading, setApi_block_2Loading] = React.useState(true);
  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch(`${API_BASE}/api/services/app/Personnel/GetAll`, { headers: getRequestHeaders() });
        // Token expired or invalid — clear auth and redirect to login
        if (res.status === 401) {
          ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k));
          if (window.location.pathname !== '/login') window.location.href = '/login';
          return;
        }
        if (!res.ok) return;
        const rawJson = await res.json();
        // Auto-unwrap ABP/generic envelopes so {result.totalCount} or bare {totalCount} both work
        const json = unwrapResponse(rawJson) as Record<string, unknown>;
        setApi_block_2Data(json);
      } catch { /* ignore */ }
      finally { setApi_block_2Loading(false); }
    };
    fetchData();
  }, []);

  const [api_block_3Data, setApi_block_3Data] = React.useState<Record<string, unknown> | null>(null);
  const [api_block_3Loading, setApi_block_3Loading] = React.useState(true);
  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch(`${API_BASE}/api/services/app/SnagReport/GetAll`, { headers: getRequestHeaders() });
        // Token expired or invalid — clear auth and redirect to login
        if (res.status === 401) {
          ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k));
          if (window.location.pathname !== '/login') window.location.href = '/login';
          return;
        }
        if (!res.ok) return;
        const rawJson = await res.json();
        // Auto-unwrap ABP/generic envelopes so {result.totalCount} or bare {totalCount} both work
        const json = unwrapResponse(rawJson) as Record<string, unknown>;
        setApi_block_3Data(json);
      } catch { /* ignore */ }
      finally { setApi_block_3Loading(false); }
    };
    fetchData();
  }, []);

  const [api_block_4Data, setApi_block_4Data] = React.useState<Record<string, unknown>[]>([]);
  const [api_block_4Loading, setApi_block_4Loading] = React.useState(true);
  React.useEffect(() => {
    const sources = [
      { name: 'Uçak', url: `${API_BASE}/api/services/app/Aircraft/GetAll?MaxResultCount=1`, path: 'result.totalCount' },
      { name: 'Personel', url: `${API_BASE}/api/services/app/Personnel/GetAll?MaxResultCount=1`, path: 'result.totalCount' },
      { name: 'Snag Raporu', url: `${API_BASE}/api/services/app/SnagReport/GetAll?MaxResultCount=1`, path: 'result.totalCount' },
    ];
    Promise.all(sources.map(async (s) => {
      try {
        const res = await fetch(s.url, { headers: getRequestHeaders() });
        if (res.status === 401) { ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k)); if (window.location.pathname !== '/login') window.location.href = '/login'; return { name: s.name, value: 0 }; }
        if (!res.ok) return { name: s.name, value: 0 };
        const json = unwrapResponse(await res.json()) as Record<string, unknown>;
        const v = getNestedValue(json, s.path);
        return { name: s.name, value: typeof v === 'number' ? v : Number(v) || 0 };
      } catch { return { name: s.name, value: 0 }; }
    })).then((rows) => { setApi_block_4Data(rows); setApi_block_4Loading(false); });
  }, []);

  const [api_block_5Data, setApi_block_5Data] = React.useState<Record<string, unknown>[]>([]);
  const [api_block_5Loading, setApi_block_5Loading] = React.useState(true);
  React.useEffect(() => {
    const sources = [
      { name: 'Uçak', url: `${API_BASE}/api/services/app/Aircraft/GetAll?MaxResultCount=1`, path: 'result.totalCount' },
      { name: 'Personel', url: `${API_BASE}/api/services/app/Personnel/GetAll?MaxResultCount=1`, path: 'result.totalCount' },
      { name: 'Snag Raporu', url: `${API_BASE}/api/services/app/SnagReport/GetAll?MaxResultCount=1`, path: 'result.totalCount' },
    ];
    Promise.all(sources.map(async (s) => {
      try {
        const res = await fetch(s.url, { headers: getRequestHeaders() });
        if (res.status === 401) { ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k)); if (window.location.pathname !== '/login') window.location.href = '/login'; return { name: s.name, value: 0 }; }
        if (!res.ok) return { name: s.name, value: 0 };
        const json = unwrapResponse(await res.json()) as Record<string, unknown>;
        const v = getNestedValue(json, s.path);
        return { name: s.name, value: typeof v === 'number' ? v : Number(v) || 0 };
      } catch { return { name: s.name, value: 0 }; }
    })).then((rows) => { setApi_block_5Data(rows); setApi_block_5Loading(false); });
  }, []);

  const [api_block_6Data, setApi_block_6Data] = React.useState<Record<string, unknown> | null>(null);
  const [api_block_6Loading, setApi_block_6Loading] = React.useState(true);
  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch(`${API_BASE}/api/services/app/Aircraft/GetAll?MaxResultCount=5&Sorting=id desc`, { headers: getRequestHeaders() });
        // Token expired or invalid — clear auth and redirect to login
        if (res.status === 401) {
          ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k));
          if (window.location.pathname !== '/login') window.location.href = '/login';
          return;
        }
        if (!res.ok) return;
        const rawJson = await res.json();
        // Auto-unwrap ABP/generic envelopes so {result.totalCount} or bare {totalCount} both work
        const json = unwrapResponse(rawJson) as Record<string, unknown>;
        const target = getNestedValue(rawJson as Record<string, unknown>, 'result.items') ?? getNestedValue(json, 'result.items');
        setApi_block_6Data(target != null ? (target as Record<string, unknown>) : json);
      } catch { /* ignore */ }
      finally { setApi_block_6Loading(false); }
    };
    fetchData();
  }, []);

  const [api_block_7Data, setApi_block_7Data] = React.useState<Record<string, unknown> | null>(null);
  const [api_block_7Loading, setApi_block_7Loading] = React.useState(true);
  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch(`${API_BASE}/api/services/app/Personnel/GetAll?MaxResultCount=5&Sorting=id desc`, { headers: getRequestHeaders() });
        // Token expired or invalid — clear auth and redirect to login
        if (res.status === 401) {
          ['_auth_token', '_bearer_token', '_refresh_token'].forEach(k => localStorage.removeItem(k));
          if (window.location.pathname !== '/login') window.location.href = '/login';
          return;
        }
        if (!res.ok) return;
        const rawJson = await res.json();
        // Auto-unwrap ABP/generic envelopes so {result.totalCount} or bare {totalCount} both work
        const json = unwrapResponse(rawJson) as Record<string, unknown>;
        const target = getNestedValue(rawJson as Record<string, unknown>, 'result.items') ?? getNestedValue(json, 'result.items');
        setApi_block_7Data(target != null ? (target as Record<string, unknown>) : json);
      } catch { /* ignore */ }
      finally { setApi_block_7Loading(false); }
    };
    fetchData();
  }, []);

  return (
    <div>
      <style dangerouslySetInnerHTML={{ __html: LOADING_KEYFRAMES }} />
      <h1 style={{ margin: '0 0 24px', fontSize: 22, fontWeight: 700 }}>Dashboard</h1>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(12, 1fr)', gap: 16 }}>
        <div style={{ gridColumn: 'span 4' }}>
          {api_block_1Loading ? (
            <TkCard>
              <div slot="content" style={{ padding: 20 }}>
                <div style={{ height: 12, width: '40%', background: '#e0e0e0', borderRadius: 4, marginBottom: 12, animation: 'pulse 1.5s ease-in-out infinite' }} />
                <div style={{ height: 28, width: '60%', background: '#e0e0e0', borderRadius: 4, marginBottom: 8, animation: 'pulse 1.5s ease-in-out infinite' }} />
                <div style={{ height: 10, width: '50%', background: '#f0f0f0', borderRadius: 4, animation: 'pulse 1.5s ease-in-out infinite' }} />
              </div>
            </TkCard>
          ) : (
          <TkCard>
            <div slot="content" style={{ padding: 20 }}>
              <div style={{ fontSize: 12, color: '#888', marginBottom: 4 }}>Total Uçak</div>
              <div style={{ fontSize: 28, fontWeight: 700, color: '#1976d2' }}>{(getNestedValue(api_block_1Data ?? {}, 'result.result.totalCount') as string | number) ?? '—'}</div>
            </div>
          </TkCard>
          )}

        </div>
        <div style={{ gridColumn: 'span 4' }}>
          {api_block_2Loading ? (
            <TkCard>
              <div slot="content" style={{ padding: 20 }}>
                <div style={{ height: 12, width: '40%', background: '#e0e0e0', borderRadius: 4, marginBottom: 12, animation: 'pulse 1.5s ease-in-out infinite' }} />
                <div style={{ height: 28, width: '60%', background: '#e0e0e0', borderRadius: 4, marginBottom: 8, animation: 'pulse 1.5s ease-in-out infinite' }} />
                <div style={{ height: 10, width: '50%', background: '#f0f0f0', borderRadius: 4, animation: 'pulse 1.5s ease-in-out infinite' }} />
              </div>
            </TkCard>
          ) : (
          <TkCard>
            <div slot="content" style={{ padding: 20 }}>
              <div style={{ fontSize: 12, color: '#888', marginBottom: 4 }}>Total Personel</div>
              <div style={{ fontSize: 28, fontWeight: 700, color: '#4caf50' }}>{(getNestedValue(api_block_2Data ?? {}, 'result.result.totalCount') as string | number) ?? '—'}</div>
            </div>
          </TkCard>
          )}

        </div>
        <div style={{ gridColumn: 'span 4' }}>
          {api_block_3Loading ? (
            <TkCard>
              <div slot="content" style={{ padding: 20 }}>
                <div style={{ height: 12, width: '40%', background: '#e0e0e0', borderRadius: 4, marginBottom: 12, animation: 'pulse 1.5s ease-in-out infinite' }} />
                <div style={{ height: 28, width: '60%', background: '#e0e0e0', borderRadius: 4, marginBottom: 8, animation: 'pulse 1.5s ease-in-out infinite' }} />
                <div style={{ height: 10, width: '50%', background: '#f0f0f0', borderRadius: 4, animation: 'pulse 1.5s ease-in-out infinite' }} />
              </div>
            </TkCard>
          ) : (
          <TkCard>
            <div slot="content" style={{ padding: 20 }}>
              <div style={{ fontSize: 12, color: '#888', marginBottom: 4 }}>Total Snag Raporu</div>
              <div style={{ fontSize: 28, fontWeight: 700, color: '#ff9800' }}>{(getNestedValue(api_block_3Data ?? {}, 'result.result.totalCount') as string | number) ?? '—'}</div>
            </div>
          </TkCard>
          )}

        </div>
        <div style={{ gridColumn: 'span 6' }}>
          {api_block_4Loading ? (
            <TkCard header="Kayıt Sayıları">
              <div slot="content" style={{ padding: 16, display: 'flex', alignItems: 'center', justifyContent: 'center', height: 280 }}>
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
                  <div style={{ width: 32, height: 32, border: '3px solid #e0e0e0', borderTopColor: '#1976d2', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
                  <span style={{ fontSize: 12, color: '#999' }}>Loading...</span>
                </div>
              </div>
            </TkCard>
          ) : (
          <TkCard header="Kayıt Sayıları">
            <div slot="content" style={{ padding: 16 }}>
              <ResponsiveContainer width="100%" height={280}>
                  <BarChart data={api_block_4Data}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="value" fill="#940000" />
                  </BarChart>
                </ResponsiveContainer>
            </div>
          </TkCard>
          )}
        </div>
        <div style={{ gridColumn: 'span 6' }}>
          {api_block_5Loading ? (
            <TkCard header="Dağılım">
              <div slot="content" style={{ padding: 16, display: 'flex', alignItems: 'center', justifyContent: 'center', height: 280 }}>
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
                  <div style={{ width: 32, height: 32, border: '3px solid #e0e0e0', borderTopColor: '#1976d2', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
                  <span style={{ fontSize: 12, color: '#999' }}>Loading...</span>
                </div>
              </div>
            </TkCard>
          ) : (
          <TkCard header="Dağılım">
            <div slot="content" style={{ padding: 16 }}>
              <ResponsiveContainer width="100%" height={280}>
                  <PieChart>
                    <Pie data={api_block_5Data} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label>
                      <Cell fill="#940000" />
                      <Cell fill="#ff9800" />
                      <Cell fill="#4caf50" />
                      <Cell fill="#e91e63" />
                      <Cell fill="#9c27b0" />
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
            </div>
          </TkCard>
          )}
        </div>
        <div style={{ gridColumn: 'span 6' }}>
          <div style={{ background: '#fff', borderRadius: 8, overflow: 'hidden', border: '1px solid #e8e8e8' }}>
            {api_block_6Loading ? (
              <div style={{ padding: 20, textAlign: 'center', color: '#999' }}>Loading...</div>
            ) : (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead><tr><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>ID</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Registration</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Aircraft Type</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Model</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Status</th></tr></thead>
                <tbody>
                  {(Array.isArray(api_block_6Data) ? api_block_6Data : extractArray(api_block_6Data)).map((row: Record<string, unknown>, i: number) => (
                    <tr key={i}><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['id'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['registration'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['aircraftType'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['model'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['status'] ?? '')}</td></tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
        <div style={{ gridColumn: 'span 6' }}>
          <div style={{ background: '#fff', borderRadius: 8, overflow: 'hidden', border: '1px solid #e8e8e8' }}>
            {api_block_7Loading ? (
              <div style={{ padding: 20, textAlign: 'center', color: '#999' }}>Loading...</div>
            ) : (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead><tr><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>ID</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>First Name</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Last Name</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Employee Number</th><th style={{ padding: '8px 12px', textAlign: 'left', borderBottom: '2px solid #e0e0e0', fontSize: 12, color: '#666' }}>Role</th></tr></thead>
                <tbody>
                  {(Array.isArray(api_block_7Data) ? api_block_7Data : extractArray(api_block_7Data)).map((row: Record<string, unknown>, i: number) => (
                    <tr key={i}><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['id'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['firstName'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['lastName'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['employeeNumber'] ?? '')}</td><td style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0', fontSize: 13 }}>{String(row['role'] ?? '')}</td></tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
