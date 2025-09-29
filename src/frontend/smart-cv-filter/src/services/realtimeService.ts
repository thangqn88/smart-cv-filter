// src/services/realtimeService.ts
import { ref, reactive } from 'vue'

export interface ProcessingStatus {
  id: string
  type: 'screening' | 'analysis' | 'upload'
  status: 'pending' | 'processing' | 'completed' | 'error'
  progress: number
  message: string
  startTime: Date
  endTime?: Date
  result?: any
  error?: string
}

export interface RealtimeUpdate {
  type: 'status_update' | 'progress_update' | 'completion' | 'error'
  data: any
  timestamp: Date
}

class RealtimeService {
  private ws: WebSocket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000
  private isConnected = ref(false)
  private processingStatuses = reactive<Map<string, ProcessingStatus>>(new Map())
  private listeners = new Map<string, Set<(update: RealtimeUpdate) => void>>()

  constructor() {
    this.connect()
  }

  private connect(): void {
    try {
      // In a real application, this would connect to your WebSocket server
      // For now, we'll simulate the connection
      this.isConnected.value = true
      console.log('Realtime service connected (simulated)')
      
      // Simulate periodic updates
      this.startSimulation()
    } catch (error) {
      console.error('Failed to connect to realtime service:', error)
      this.scheduleReconnect()
    }
  }

  private startSimulation(): void {
    // Simulate real-time updates for demonstration
    setInterval(() => {
      this.simulateUpdate()
    }, 3000)
  }

  private simulateUpdate(): void {
    // Simulate various types of updates
    const updateTypes = ['status_update', 'progress_update', 'completion', 'error']
    const type = updateTypes[Math.floor(Math.random() * updateTypes.length)]
    
    const update: RealtimeUpdate = {
      type: type as any,
      data: this.generateSimulatedData(type),
      timestamp: new Date()
    }

    this.broadcastUpdate(update)
  }

  private generateSimulatedData(type: string): any {
    switch (type) {
      case 'status_update':
        return {
          id: `processing_${Date.now()}`,
          status: 'processing',
          message: 'AI is analyzing CV...'
        }
      case 'progress_update':
        return {
          id: `processing_${Date.now()}`,
          progress: Math.floor(Math.random() * 100),
          message: 'Processing in progress...'
        }
      case 'completion':
        return {
          id: `processing_${Date.now()}`,
          status: 'completed',
          result: {
            overallScore: Math.floor(Math.random() * 40) + 60,
            summary: 'Analysis completed successfully'
          }
        }
      case 'error':
        return {
          id: `processing_${Date.now()}`,
          status: 'error',
          error: 'Processing failed due to invalid file format'
        }
      default:
        return {}
    }
  }

  private scheduleReconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      setTimeout(() => {
        this.reconnectAttempts++
        this.connect()
      }, this.reconnectDelay * this.reconnectAttempts)
    }
  }

  private broadcastUpdate(update: RealtimeUpdate): void {
    // Broadcast to all listeners
    this.listeners.forEach((listeners) => {
      listeners.forEach((listener) => {
        try {
          listener(update)
        } catch (error) {
          console.error('Error in realtime listener:', error)
        }
      })
    })
  }

  // Public methods
  public subscribe(eventType: string, callback: (update: RealtimeUpdate) => void): () => void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, new Set())
    }
    
    this.listeners.get(eventType)!.add(callback)
    
    // Return unsubscribe function
    return () => {
      const listeners = this.listeners.get(eventType)
      if (listeners) {
        listeners.delete(callback)
        if (listeners.size === 0) {
          this.listeners.delete(eventType)
        }
      }
    }
  }

  public startProcessing(processingId: string, type: ProcessingStatus['type'], message: string): void {
    const status: ProcessingStatus = {
      id: processingId,
      type,
      status: 'pending',
      progress: 0,
      message,
      startTime: new Date()
    }
    
    this.processingStatuses.set(processingId, status)
    
    // Simulate processing start
    setTimeout(() => {
      this.updateProcessingStatus(processingId, 'processing', 10, 'Starting analysis...')
    }, 1000)
  }

  public updateProcessingStatus(
    processingId: string, 
    status: ProcessingStatus['status'], 
    progress: number, 
    message: string
  ): void {
    const currentStatus = this.processingStatuses.get(processingId)
    if (currentStatus) {
      currentStatus.status = status
      currentStatus.progress = progress
      currentStatus.message = message
      
      if (status === 'completed' || status === 'error') {
        currentStatus.endTime = new Date()
      }
      
      this.broadcastUpdate({
        type: 'status_update',
        data: currentStatus,
        timestamp: new Date()
      })
    }
  }

  public completeProcessing(processingId: string, result: any): void {
    this.updateProcessingStatus(processingId, 'completed', 100, 'Processing completed')
    
    const currentStatus = this.processingStatuses.get(processingId)
    if (currentStatus) {
      currentStatus.result = result
      currentStatus.endTime = new Date()
      
      this.broadcastUpdate({
        type: 'completion',
        data: currentStatus,
        timestamp: new Date()
      })
    }
  }

  public failProcessing(processingId: string, error: string): void {
    this.updateProcessingStatus(processingId, 'error', 0, 'Processing failed')
    
    const currentStatus = this.processingStatuses.get(processingId)
    if (currentStatus) {
      currentStatus.error = error
      currentStatus.endTime = new Date()
      
      this.broadcastUpdate({
        type: 'error',
        data: currentStatus,
        timestamp: new Date()
      })
    }
  }

  public getProcessingStatus(processingId: string): ProcessingStatus | undefined {
    return this.processingStatuses.get(processingId)
  }

  public getAllProcessingStatuses(): ProcessingStatus[] {
    return Array.from(this.processingStatuses.values())
  }

  public clearProcessingStatus(processingId: string): void {
    this.processingStatuses.delete(processingId)
  }

  public clearAllProcessingStatuses(): void {
    this.processingStatuses.clear()
  }

  public disconnect(): void {
    if (this.ws) {
      this.ws.close()
      this.ws = null
    }
    this.isConnected.value = false
  }

  // Getters
  public get connected(): boolean {
    return this.isConnected.value
  }

  public get processingCount(): number {
    return this.processingStatuses.size
  }

  public get activeProcessingCount(): number {
    return Array.from(this.processingStatuses.values())
      .filter(status => status.status === 'processing' || status.status === 'pending').length
  }
}

// Create and export a singleton instance
export const realtimeService = new RealtimeService()

// Export the class for testing
export { RealtimeService }
