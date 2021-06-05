//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using System.Collections.Generic;

namespace DungeonArchitect.Utils
{
    public class StackSystem<TState, TStaticState, TResult>
    {   
        protected TStaticState staticState;

        private Stack<TState> stack = new Stack<TState>();
        private bool running = false;
        private bool foundResult = false;
        private TResult result;

        public delegate void ExecuteFrameDelegate(TState top, TStaticState staticState, StackSystem<TState, TStaticState, TResult> stackSystem);

        public StackSystem(TStaticState staticState)
        {
            this.staticState = staticState;
        }
        
        public bool Running
        {
            get => running;
        }

        public bool FoundResult
        {
            get => foundResult;
        }

        public TResult Result
        {
            get => result;
        }

        public Stack<TState> Stack
        {
            get => stack;
        }

        public void Initialize(TState state)
        {
            stack.Push(state);
            running = true;
        }

        public void PushFrame(TState state)
        {
            stack.Push(state);
        }

        public void FinalizeResult(TResult result)
        {
            running = false;
            foundResult = true;
            this.result = result;
        }

        public void Halt()
        {
            running = false;
        }

        public void ExecuteStep(ExecuteFrameDelegate executeFrame)
        {
            if (stack.Count == 0)
            {
                running = false;
            }

            if (!running)
            {
                return;
            }

            var top = stack.Pop();
            if (executeFrame != null)
            {
                executeFrame.Invoke(top, staticState, this);
            }
        }
    }
}